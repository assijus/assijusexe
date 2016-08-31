Module Launch

    Public Sub Main(ByVal cmdArgs() As String)
        Dim thisRun As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess()
        Dim pList() As System.Diagnostics.Process = System.Diagnostics.Process.GetProcessesByName("Assijus")


        For Each eachRun As System.Diagnostics.Process In pList
            If (Not thisRun.Id = eachRun.Id) Then
                eachRun.Kill()  ' Kill all other runs, but not this current one
            End If
        Next eachRun

        'Dim regStartUp As RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)

        'Dim value As String

        'value = regStartUp.GetValue("BluCRESTSigner")

        'If value <> Application.ExecutablePath.ToString() Then

        '    regStartUp.CreateSubKey("BluCRESTSigner")
        '    regStartUp.SetValue("BluCRESTSigner", Application.ExecutablePath.ToString())

        'End If

        Application.EnableVisualStyles()
        Application.Run(New AppContext)


    End Sub
End Module
