Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports System.Web.Script.Serialization
Imports System.IO
Imports System.Deployment.Application
Imports WebSocket4Net

Public Class AppContext
    Inherits ApplicationContext

#Region " Storage "

    Private WithEvents Tray As NotifyIcon
    Private WithEvents MainMenu As ContextMenuStrip
    Private WithEvents mnuDisplayForm As ToolStripMenuItem
    Private WithEvents mnuSep1 As ToolStripSeparator
    Private WithEvents mnuConnect As ToolStripMenuItem
    Private WithEvents mnuSep2 As ToolStripSeparator
    Private WithEvents mnuExit As ToolStripMenuItem
    Private WithEvents mnuShowSite As ToolStripMenuItem
    Private WithEvents mnuShowForm As ToolStripMenuItem

    Private httpListener As HttpListener
    Private tcpListener As TcpListener
    Private listenThread As Thread
    Private connectedClients As Integer = 0
    Private Delegate Sub WriteMessageDelegate(ByVal msg As String)

    Private frm As MainForm = New MainForm
    Private websocket As WebSocket4Net.WebSocket

    Private assijusurl As String = "https://assijus.jfrj.jus.br"

#End Region

#Region " Constructor "

    Public Sub New()
        Dim arguments As String() = Environment.GetCommandLineArgs()

        If arguments.Length >= 2 Then
            assijusurl = arguments(1)
        End If

        'Initialize the menus
        mnuShowSite = New ToolStripMenuItem("Abrir Assijus Web Site")
        mnuShowSite.Font = New Font(mnuShowSite.Font, mnuShowSite.Font.Style Or FontStyle.Bold)
        mnuShowForm = New ToolStripMenuItem("Abrir Assijus Windows")
        mnuSep1 = New ToolStripSeparator()
        mnuConnect = New ToolStripMenuItem("WebSocket")
        ' mnuConnect.CheckOnClick = True
        mnuSep2 = New ToolStripSeparator()
        mnuExit = New ToolStripMenuItem("Sair")
        MainMenu = New ContextMenuStrip
        MainMenu.Items.AddRange(New ToolStripItem() {mnuShowSite, mnuShowForm, mnuSep1, mnuConnect, mnuSep2, mnuExit})

        'Initialize the tray
        Tray = New NotifyIcon
        'Tray.Icon = My.Resources.TrayIcon
        Tray.Icon = My.Resources.favicon
        Tray.ContextMenuStrip = MainMenu
        Try
            Tray.Text = Application.ProductName & " v" & ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString
        Catch ex As Exception
            Tray.Text = Application.ProductName
        End Try

        'Display
        Tray.Visible = True

        frm.Show()
        frm.Hide()
        frm.ctx = Me

        Server()
        'HttpServer()
        launchAssijusWebSite()

        Try
            websocket = New WebSocket4Net.WebSocket("ws://localhost:8080/assijus/websocket/server")
            AddHandler websocket.Opened, Sub(s, e) socketOpened(s, e)
            AddHandler websocket.Error, Sub(s, e) socketError(s, e)
            AddHandler websocket.Closed, Sub(s, e) socketClosed(s, e)
            AddHandler websocket.MessageReceived, Sub(s, e) socketMessage(s, e)
            AddHandler websocket.DataReceived, Sub(s, e) socketDataReceived(s, e)
            websocket.Open()
        Catch ex As Exception
            showMsg("Erro abrindo socket: " & ex.Message)
        End Try
    End Sub

    Sub socketOpened(s As WebSocket, e As EventArgs)
        s.Send("{""kind"":""HELLO"",""certificate"":""" & Assijus.currentcert() & """,""app"":""signer""}")
        showMsg("Conectado!")
        SetConnectChecked(True)
    End Sub

    Sub socketClosed(s As WebSocket, e As EventArgs)
        If mnuConnect.Checked Then
            showMsg("Desconectado!")
            SetConnectChecked(False)
            s.Open()
        End If
    End Sub

    Sub socketError(s As WebSocket, e As SuperSocket.ClientEngine.ErrorEventArgs)
        showMsg("Erro de comunicação: " + e.Exception.Message)
        showMsg("Erro: " + e.Exception.Message)
    End Sub

    Sub socketMessage(s As WebSocket, e As WebSocket4Net.MessageReceivedEventArgs)
        If e.Message.Contains("""kind"":""START""") OrElse e.Message.Contains("""kind"": ""START""") Then
            batchsign(e.Message)
        ElseIf e.Message.Contains("""kind"":""PING""") OrElse e.Message.Contains("""kind"": ""PING""") Then
            s.Send("{""kind"":""PONG""}")
        End If
    End Sub


    Sub socketDataReceived(ss As Object, e As WebSocket4Net.DataReceivedEventArgs)

    End Sub

    Public Sub showMsg(msg As String)

        Tray.BalloonTipTitle = "Assijus"
        Tray.BalloonTipText = msg
        Tray.BalloonTipIcon = ToolTipIcon.None
        Tray.ShowBalloonTip(1000)
    End Sub

    Public Sub wsSend(json As String)
        Try
            If Not IsNothing(websocket) Then
                websocket.Send(json)
            End If
        Catch ex As Exception
            Console.WriteLine(json)
        End Try
    End Sub

#End Region


#Region " HTTP Server "
    Private Sub HttpServer()
        Me.httpListener = New HttpListener() ' Change to IPAddress.Any for internet wide Communication ou Loopback for localhost only

        Me.listenThread = New Thread(New ThreadStart(AddressOf HttpListenForClients))
        Me.listenThread.Start()
    End Sub

    Private Sub HttpListenForClients()
        httpListener.Prefixes.Add("http://localhost:8612/")
        'httpListener.Prefixes.Add("http://csis-trf-10.corp.jfrj.gov.br:8612/")
        'httpListener.Prefixes.Add("http://10.34.15.113:8612/")
        'httpListener.Prefixes.Add("http://*:8612/")
        httpListener.Start()

        Do
            Try
                Dim ctx As HttpListenerContext = httpListener.GetContext()

                Dim clientThread As New Thread(New ParameterizedThreadStart(AddressOf HttpHandleClientComm))
                clientThread.Start(ctx)
            Catch
                Return
            End Try
        Loop
    End Sub

    Private Sub HttpHandleClientComm(ByVal ctx As HttpListenerContext)
        Dim clientStream As Stream = ctx.Request.InputStream

        Dim content(ctx.Request.ContentLength64 - 1) As Byte
        Dim length As Integer = content.Length
        Dim read As Integer = 0
        Dim offset As Integer = 0

        Do
            read = clientStream.Read(content, offset, length - offset)
            offset += read
        Loop Until offset = length

        Dim encoder As New UTF8Encoding()
        Dim msg As String = encoder.GetString(content, 0, content.Length)
        HttpRun(msg, encoder, ctx)
    End Sub

    Private Sub HttpRun(ByVal msg As String, ByVal encoder As UTF8Encoding, ByVal ctx As HttpListenerContext)
        ctx.Response.AddHeader("Connection", "close")
        ctx.Response.AddHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept")
        ctx.Response.AddHeader("Access-Control-Allow-Origin", "*")
        ctx.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST")
        ctx.Response.AddHeader("Content-Type", "application/json")
        ctx.Response.AddHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept")
        ctx.Response.AddHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept")

#If DEBUG Then
        'headers += "Access-Control-Allow-Origin: http://localhost:8888" + vbCrLf
#Else
        'headers += "Access-Control-Allow-Origin: " & assijusurl + vbCrLf
#End If

        Try
            Dim jsonOut As String = ""
            Dim buffer() As Byte
            Dim parts As String() = Split(msg, vbCrLf + "" + vbCrLf)
            Dim jsonIn As String = msg

            If ctx.Request.HttpMethod = "GET" AndAlso ctx.Request.Url.AbsolutePath = "/test" Then
                jsonOut = test()
            ElseIf ctx.Request.HttpMethod = "GET" AndAlso ctx.Request.Url.AbsolutePath = "/currentcert" Then
                jsonOut = currentcert()
            ElseIf (ctx.Request.HttpMethod = "GET" Or ctx.Request.HttpMethod = "POST") AndAlso ctx.Request.Url.AbsolutePath = "/cert" Then
                jsonOut = cert(jsonIn)
            ElseIf ctx.Request.HttpMethod = "OPTIONS" Then
                jsonOut = options()
            ElseIf ctx.Request.HttpMethod = "POST" AndAlso ctx.Request.Url.AbsolutePath = "/token" Then
                jsonOut = token(jsonIn)
            ElseIf ctx.Request.HttpMethod = "POST" AndAlso ctx.Request.Url.AbsolutePath = "/sign" Then
                jsonOut = sign(jsonIn)
            ElseIf ctx.Request.HttpMethod = "POST" AndAlso ctx.Request.Url.AbsolutePath = "/batchsign" Then
                jsonOut = batchsign(jsonIn)
            Else
                ctx.Response.StatusCode = 404
                Dim buffer404() As Byte = encoder.GetBytes("{""errormsg"":""Error 404: file not found""}")
                ctx.Response.ContentLength64 = buffer404.Length
                ctx.Response.OutputStream.Write(buffer404, 0, buffer404.Length)
                Return
            End If

            ctx.Response.StatusCode = 200
            buffer = encoder.GetBytes(jsonOut)
            ctx.Response.ContentLength64 = buffer.Length
            ctx.Response.OutputStream.Write(buffer, 0, buffer.Length)
        Catch ex As Exception
            ctx.Response.StatusCode = 500
            Dim buffer500() As Byte
            Dim message As String = ex.Message
            If message.StartsWith("O conjunto de chaves não") Then
                message = "Não localizamos nenhum Token válido no computador. Por favor, verifique se foi corretamente inserido."
            End If

            buffer500 = encoder.GetBytes("{""errormsg"":""" + jsonStringSafe(message) + """}")
            ctx.Response.ContentLength64 = buffer500.Length
            ctx.Response.OutputStream.Write(buffer500, 0, buffer500.Length)
            Return
        End Try

    End Sub

#End Region

#Region " TCP Server (Deprecated) "

    Private Sub Server()
#If DEBUG Then
        Dim ipa = IPAddress.Any
#Else
        Dim ipa = IPAddress.Loopback
#End If
        Me.tcpListener = New TcpListener(ipa, 8612) ' Change to IPAddress.Any for internet wide Communication ou Loopback for localhost only
        Me.listenThread = New Thread(New ThreadStart(AddressOf ListenForClients))
        Me.listenThread.Start()
    End Sub

    Private Sub ListenForClients()
        Me.tcpListener.Start()

        Do
            Try
                Dim client As TcpClient = Me.tcpListener.AcceptTcpClient()
                connectedClients += 1
                Dim clientThread As New Thread(New ParameterizedThreadStart(AddressOf HandleClientComm))
                clientThread.Start(client)
            Catch
                Return
            End Try
        Loop
    End Sub

    Private Sub HandleClientComm(ByVal client As Object)
        Dim tcpClient As TcpClient = DirectCast(client, TcpClient)
        Dim clientStream As NetworkStream = tcpClient.GetStream()

        Dim message(100000) As Byte
        Dim bytesRead As Integer
        Dim c As Integer = 0
        Dim contentLength As Integer = 0
        Dim msg As String = ""

        'message has successfully been received
        Dim encoder As New UTF8Encoding()

        Do
            bytesRead = 0

            Try
                'blocks until a client sends a message
                bytesRead = clientStream.Read(message, c, 1)
                c += bytesRead
            Catch
                'a socket error has occured
                Exit Do
            End Try

            If bytesRead = 0 Then
                Exit Do
            End If

            If c >= 4 AndAlso message(c - 4) = 13 AndAlso message(c - 3) = 10 AndAlso message(c - 2) = 13 AndAlso message(c - 1) = 10 Then
                Dim header As String = encoder.GetString(message, 0, c)
                msg += header
                Dim lines As String() = header.Split(vbLf)
                For Each line In lines
                    If line.StartsWith("Content-Length:") Then
                        contentLength = Integer.Parse(line.Substring(16))
                    End If
                Next

                If contentLength = 0 Then
                    Exit Do
                End If

                Dim content(contentLength - 1) As Byte
                Dim length As Integer = content.Length
                Dim read As Integer = 0
                Dim offset As Integer = 0

                Do
                    read = clientStream.Read(content, offset, length - offset)
                    offset += read
                    'Console.Out.WriteLine("offset: " & offset & ", read: " & read)
                Loop Until offset = length

                msg += encoder.GetString(content, 0, content.Length)
                Exit Do
            End If

        Loop

        'Console.Out.Write(msg)

        Run(msg, encoder, clientStream)

        tcpClient.Close()
    End Sub

    Private Sub HandleClientCommOld(ByVal client As Object)
        Dim tcpClient As TcpClient = DirectCast(client, TcpClient)
        Dim clientStream As NetworkStream = tcpClient.GetStream()

        Dim message(100000) As Byte
        Dim bytesRead As Integer

        Do
            bytesRead = 0

            Try
                'blocks until a client sends a message
                bytesRead = clientStream.Read(message, 0, 100000)
            Catch
                'a socket error has occured
                Exit Do
            End Try

            If bytesRead = 0 Then
                'the client has disconnected from the server
                connectedClients -= 1
                'lblNumberOfConnections.Text = connectedClients.ToString()
                Exit Do
            End If

            'message has successfully been received
            Dim encoder As New UTF8Encoding()

            ' Convert the Bytes received to a string and display it on the Server Screen
            Dim msg As String = encoder.GetString(message, 0, bytesRead)

            Run(msg, encoder, clientStream)
        Loop

        tcpClient.Close()
    End Sub

    Private Sub HandleClientCommNewer(ByVal client As Object)
        Dim tcpClient As TcpClient = DirectCast(client, TcpClient)
        Dim clientStream As NetworkStream = tcpClient.GetStream()
        Dim encoder As New UTF8Encoding()

        Dim reader As New IO.StreamReader(clientStream, System.Text.Encoding.ASCII, False, 1)
        Dim contentLength As Integer
        Dim msg As String = ""
        Do
            Dim line As String = reader.ReadLine
            If IsNothing(line) Then
                Exit Do
            End If
            msg += line + vbCrLf
            If line.StartsWith("Content-Length:") Then
                contentLength = Integer.Parse(line.Substring(16))
            ElseIf line = "" Then
                If contentLength = 0 Then
                    Exit Do
                End If

                Dim content(contentLength - 1) As Byte
                clientStream.Read(content, 0, content.Length)
                msg += encoder.GetString(content, 0, content.Length)
                Exit Do
            End If
        Loop

        Run(msg, encoder, clientStream)

        tcpClient.Close()
    End Sub



    Private Sub Run(ByVal msg As String, ByVal encoder As UTF8Encoding, ByVal clientStream As NetworkStream)
        Dim headers As String = vbCrLf + "Connection: close" + vbCrLf
        headers += "Access-Control-Allow-Headers: Origin, X-Requested-With, Content-Type, Accept" + vbCrLf
        headers += "Access-Control-Allow-Origin: *" + vbCrLf
#If DEBUG Then
        'headers += "Access-Control-Allow-Origin: http://localhost:8888" + vbCrLf
#Else
        'headers += "Access-Control-Allow-Origin: http://trf2signer.appspot.com" + vbCrLf
#End If
        headers += "Access-Control-Allow-Methods: GET, POST" + vbCrLf
        headers += "Content-Type: application/json" + vbCrLf + vbCrLf
        Try
            Dim jsonOut As String = ""
            Dim buffer() As Byte
            Dim parts As String() = Split(msg, vbCrLf + "" + vbCrLf)
            Dim jsonIn As String = parts(1)

            If msg.StartsWith("GET /test") Then
                jsonOut = test()
            ElseIf msg.StartsWith("GET /currentcert") Then
                jsonOut = currentcert()
            ElseIf msg.StartsWith("GET /cert") Or msg.StartsWith("POST /cert") Then
                jsonOut = cert(jsonIn)
            ElseIf msg.StartsWith("OPTIONS /token") Then
                jsonOut = options()
            ElseIf msg.StartsWith("POST /token") Then
                jsonOut = token(jsonIn)
            ElseIf msg.StartsWith("OPTIONS /sign") Or msg.StartsWith("OPTIONS /batchsign") Then
                jsonOut = options()
            ElseIf msg.StartsWith("POST /sign") Then
                jsonOut = sign(jsonIn)
            ElseIf msg.StartsWith("POST /batchsign") Then
                jsonOut = batchsign(jsonIn)
            Else
                Dim header404 As String = "HTTP/1.x 404 NOT FOUND" + headers + "{""errormsg"":""Error 404: file not found""}"
                buffer = encoder.GetBytes(header404)
                clientStream.Write(buffer, 0, buffer.Length)
                clientStream.Flush()
                clientStream.Close()
                Return
            End If

            Dim header As String = "HTTP/1.x 200 OK" + headers

            buffer = encoder.GetBytes(header)
            clientStream.Write(buffer, 0, buffer.Length)

            buffer = encoder.GetBytes(jsonOut)

            clientStream.Write(buffer, 0, buffer.Length)
            clientStream.Flush()
            clientStream.Close()
        Catch ex As Exception
            Try
                Dim buffer() As Byte
                Dim message As String = ex.Message
                Dim header500 As String = "HTTP/1.x 500 SERVER ERROR" + headers + "{""errormsg"":""" + jsonStringSafe(message) + """}"
                buffer = encoder.GetBytes(header500)
                clientStream.Write(buffer, 0, buffer.Length)
                clientStream.Flush()
                clientStream.Close()
                Return
            Catch exc As Exception
                clientStream.Close()
            End Try
        End Try
    End Sub

#End Region

#Region " Methods "

    Function options() As String
        Return ""
    End Function

    Function test() As String
        Dim jsonSerializer As New JavaScriptSerializer

        Dim testresponse As New TestResponse
        testresponse.provider = Application.ProductName
        testresponse.version = Application.ProductVersion
        testresponse.status = "OK"
        Dim jsonOut As String = jsonSerializer.Serialize(testresponse)

        Return jsonOut
    End Function

    Function currentcert() As String
        Dim jsonSerializer As New JavaScriptSerializer

        Dim certificateresponse As New CertificateResponse
        certificateresponse.subject = getSubject()
        If Not String.IsNullOrEmpty(certificateresponse.subject) Then
            certificateresponse.certificate = getCertificate("Assinatura Digital", "Escolha o certificado que será utilizado na assinatura.", certificateresponse.subject, "")
            certificateresponse.subject = getSubject()
        End If

        If String.IsNullOrEmpty(certificateresponse.subject) Then
            certificateresponse.subject = Nothing
            certificateresponse.errormsg = "Nenhum certificado ativo no momento."
        End If

        Dim jsonOut As String = jsonSerializer.Serialize(certificateresponse)

        Return jsonOut
    End Function

    Function cert(jsonIn As String) As String
        Dim jsonSerializer As New JavaScriptSerializer

        Dim certificaterequest As CertificateRequest = jsonSerializer.Deserialize(Of CertificateRequest)(jsonIn)
        Dim subjectRegEx As String = "ICP-Brasil"

        If (Not certificaterequest Is Nothing) AndAlso (Not String.IsNullOrEmpty(certificaterequest.subject)) Then
            subjectRegEx = certificaterequest.subject
        End If

        Dim certificateresponse As New CertificateResponse
        certificateresponse.certificate = getCertificate("Assinatura Digital", "Escolha o certificado que será utilizado na assinatura.", subjectRegEx, "")
        certificateresponse.subject = getSubject()

        If String.IsNullOrEmpty(certificateresponse.certificate) Then
            certificateresponse.errormsg = "Nenhum certificado encontrado."
        End If

        Dim jsonOut As String = jsonSerializer.Serialize(certificateresponse)

        Return jsonOut
    End Function

    Function token(jsonIn As String) As String
        Dim jsonSerializer As New JavaScriptSerializer

        Dim tokenrequest As TokenRequest = jsonSerializer.Deserialize(Of TokenRequest)(jsonIn)

        Try
            If tokenrequest.subject <> Nothing Then
                Dim s As String = BluC.getCertificateBySubject(tokenrequest.subject)
            End If

            If (Not tokenrequest.token.StartsWith("TOKEN-")) Then
                Throw New System.Exception("Token should start with TOKEN-.")
            End If
            If (tokenrequest.token.Length > 128 Or tokenrequest.token.Length < 16) Then
                Throw New System.Exception("Token too long or too shor.")
            End If

            Dim datetime() As Byte = Encoding.UTF8.GetBytes(tokenrequest.token)
            ' Dim subject() As Byte = Encoding.UTF8.GetBytes(tokenrequest.subject)
            'Dim payload(datetime.Length + subject.Length - 1) As Byte
            'Buffer.BlockCopy(datetime, 0, payload, 0, datetime.Length)
            'Buffer.BlockCopy(subject, 0, payload, datetime.Length, subject.Length)
            Dim payloadAsString As String = Convert.ToBase64String(datetime)

            Dim tokenresponse As New TokenResponse
            tokenresponse.sign = BluC.sign(99, payloadAsString)

            tokenresponse.subject = getSubject()
            tokenresponse.token = tokenrequest.token

            Dim jsonOut As String = jsonSerializer.Serialize(tokenresponse)
            Return jsonOut
        Catch ex As Exception
            BluC.clearCurrentCertificate()
            Throw ex
        End Try
    End Function

    Function sign(jsonIn As String) As String
        'Throw New SystemException("error...")
        Dim jsonSerializer As New JavaScriptSerializer

        Dim signrequest As SignRequest = jsonSerializer.Deserialize(Of SignRequest)(jsonIn)

        showMsg("Assinando: " + signrequest.code)

        If signrequest.subject <> Nothing Then
            Dim s As String = BluC.getCertificateBySubject(signrequest.subject)
        End If

        Dim keySize = getKeySize()
        Dim signresponse As New SignResponse
        If signrequest.policy = "PKCS7" Then
            signresponse.sign = BluC.sign(99, signrequest.payload)
        ElseIf keySize < 2048 Then
            signresponse.sign = BluC.sign("sha1", signrequest.payload)
        Else
            signresponse.sign = BluC.sign("sha256", signrequest.payload)
        End If

        signresponse.subject = getSubject()

#If Not DEBUG Then
        Dim storeresponse As StoreResponse = store(signresponse.sign)
        If storeresponse.status <> "OK" Then
            Throw New SystemException(storeresponse.errormsg)
        End If
        signresponse.signkey = storeresponse.key
        signresponse.sign = ""
#End If

        Dim jsonOut As String = jsonSerializer.Serialize(signresponse)
        Return jsonOut
    End Function

    Private Function store(payload As String) As StoreResponse
        Dim jsonSerializer As New JavaScriptSerializer

        Dim storerequest As New StoreRequest

        storerequest.payload = payload
        Dim jsonpayloadrequest As String = jsonSerializer.Serialize(storerequest)
        Dim byteArray As Byte() = Encoding.UTF8.GetBytes(jsonpayloadrequest)

        Dim request As WebRequest = WebRequest.Create(assijusurl & "/assijus/api/v1/store")
        request.Method = "POST"
        request.ContentType = "application/json"
        request.ContentLength = byteArray.Length
        Dim dataStream As Stream = request.GetRequestStream()
        request.GetRequestStream()
        dataStream.Write(byteArray, 0, byteArray.Length)
        Dim response As WebResponse = request.GetResponse()
        Dim reader As New StreamReader(response.GetResponseStream())
        Dim jsonOperationGetResponse As String = reader.ReadToEnd()
        Return jsonSerializer.Deserialize(Of StoreResponse)(jsonOperationGetResponse)
    End Function

    Private Function retrieve(key As String) As RetrieveResponse
        Dim jsonSerializer As New JavaScriptSerializer

        Dim retrieverequest As New RetrieveRequest

        retrieverequest.key = key
        Dim jsonpayloadrequest As String = jsonSerializer.Serialize(retrieverequest)
        Dim byteArray As Byte() = Encoding.UTF8.GetBytes(jsonpayloadrequest)

        Dim request As WebRequest = WebRequest.Create(assijusurl & "/assijus/api/v1/retrieve")
        request.Method = "POST"
        request.ContentType = "application/json"
        request.ContentLength = byteArray.Length
        Dim dataStream As Stream = request.GetRequestStream()
        request.GetRequestStream()
        dataStream.Write(byteArray, 0, byteArray.Length)
        Dim response As WebResponse = request.GetResponse()
        Dim reader As New StreamReader(response.GetResponseStream())
        Dim jsonOperationGetResponse As String = reader.ReadToEnd()
        Return jsonSerializer.Deserialize(Of RetrieveResponse)(jsonOperationGetResponse)
    End Function

    Function batchsign(jsonIn As String) As String
        Try
            Dim jsonSerializer As New JavaScriptSerializer

            Dim batchsignrequest As BatchSignRequest = jsonSerializer.Deserialize(Of BatchSignRequest)(jsonIn)

            If batchsignrequest.key <> Nothing Then
                Dim rr As RetrieveResponse = retrieve(batchsignrequest.key)
                batchsignrequest = jsonSerializer.Deserialize(Of BatchSignRequest)(rr.payload)
            End If

            If batchsignrequest.subject <> Nothing Then
                Dim s As String = BluC.getCertificateBySubject(batchsignrequest.subject)
            End If

            Dim batchsignresponse As New BatchSignResponse

            batchsignresponse.subject = getSubject()

            frm.docs = batchsignrequest.list

            frm.Invoke(frm.BatchSignDelegate)
            Dim jsonOut As String = jsonSerializer.Serialize(batchsignresponse)
            Return jsonOut
        Catch ex As Exception
            Dim rex As RestException = New RestException("iniciar lote", ex)
            wsSend("{""kind"":""FAILED"",""response"":" & rex.ToJSON() & "}")
        End Try
    End Function

    Private Function jsonStringSafe(s As String) As String
        s = s.Replace(vbCr, " ")
        s = s.Replace(vbLf, " ")
        Return s
    End Function

    Private Class TestResponse
        Public provider As String
        Public version As String
        Public status As String
        Public errormsg As String
    End Class

    Private Class CertificateRequest
        Public certificate As String
        Public subject As String
    End Class

    Private Class CertificateResponse
        Public certificate As String
        Public subject As String
        Public errormsg As String
    End Class

    Private Class SignRequest
        Public payload As String
        Public certificate As String
        Public subject As String
        Public policy As String
        Public code As String
    End Class

    Private Class SignResponse
        Public sign As String
        Public signkey As String
        Public subject As String
        Public errormsg As String
    End Class

    Private Class TokenRequest
        Public token As String
        Public certificate As String
        Public subject As String
        Public policy As String
    End Class

    Private Class TokenResponse
        Public sign As String
        Public token As String
        Public subject As String
        Public errormsg As String
    End Class


    Private Class StoreRequest
        Public payload As String
    End Class

    Private Class StoreResponse
        Public key As String
        Public status As String
        Public errormsg As String
    End Class

    Private Class RetrieveRequest
        Public key As String
    End Class

    Private Class RetrieveResponse
        Public payload As String
        Public status As String
        Public errormsg As String
    End Class

    Private Class BatchSignRequest
        Public certificate As String
        Public subject As String
        Public key As String
        Public list() As Assijus.Doc
    End Class

    Private Class BatchSignResponse
        Public subject As String
        Public errormsg As String
    End Class

#End Region

#Region " Event handlers "

    Private Sub AppContext_ThreadExit(ByVal sender As Object, ByVal e As System.EventArgs) _
    Handles Me.ThreadExit
        Tray.Visible = False
    End Sub


    Private Sub mnuExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
    Handles mnuExit.Click
        Try
            Me.httpListener.Prefixes.Clear()
            Me.httpListener.Stop()
        Catch ex As Exception
        End Try

        Try
            Me.tcpListener.Stop()
        Catch ex As Exception
        End Try

        Application.Exit()
    End Sub

    Private Sub mnuShowSite_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
    Handles mnuShowSite.Click
        launchAssijusWebSite()
    End Sub

    Private Sub launchAssijusWebSite()
        Assijus.currentcert()
        Dim token As String = Assijus.getToken()
        Dim key As String = Assijus.getAuthKey()

        Dim webAddress As String = assijusurl.Replace("https://", "http://") & "/assijus?authkey=" & key
        Process.Start(webAddress)
    End Sub

    Private Sub mnuShowForm_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
    Handles mnuShowForm.Click
        Try
            frm.Show()
        Catch ex As Exception
            frm = New MainForm
            frm.Show()
        End Try
    End Sub

    Private Sub mnuConnect_Click(ByVal sender As Object, ByVal e As System.EventArgs) _
    Handles mnuConnect.Click
        If Not mnuConnect.Checked Then
            websocket.Open()
        Else
            mnuConnect.Checked = False
            showMsg("Desconectado pelo usuário.")
            websocket.Close()
        End If
    End Sub

    Delegate Sub SetConnectCallback([val] As Boolean)
    Private Sub SetConnectChecked(ByVal [val] As Boolean)

        ' InvokeRequired required compares the thread ID of the
        ' calling thread to the thread ID of the creating thread.
        ' If these threads are different, it returns true.
        If Tray.ContextMenuStrip.InvokeRequired Then
            Dim d As New SetConnectCallback(AddressOf SetConnectChecked)
            Tray.ContextMenuStrip.Invoke(d, New Object() {[val]})
        Else
            mnuConnect.Checked = [val]
        End If
    End Sub

#End Region

End Class