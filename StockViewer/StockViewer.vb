Imports System.ComponentModel

Public Class StockViewer
    Private Structure StockName
        Public Code As String
        Public Market As String
    End Structure

    Private LocalMachine As Microsoft.Win32.RegistryKey = My.Computer.Registry.LocalMachine
    Private SoftWareKey As Microsoft.Win32.RegistryKey
    Private StockViewerKey As Microsoft.Win32.RegistryKey
    Private CodesKey As Microsoft.Win32.RegistryKey
    Private Stocks As New List(Of StockName)()
    Private StockIndex As Integer = 0
    Private FormBitmap As Bitmap
    Private FormGraphics As Graphics
    Private FormRectangle As Rectangle
    Private PosCode, PosLast As Point
    Private FormOpacity As Double = 0.5
    Private DragState As Boolean = False
    Private DragPos As Point
    Private MousePos As Point
    Private LabelCloseState As Boolean = False
    Private QueryThread As Threading.Thread

    Delegate Sub Callback(code As String, last As Double, change As Double, dec As Integer)

    Private Sub UpdateAfterQuery(code As String, last As Double, change As Double, Optional dec As Integer = 2)
        FormGraphics.FillRectangle(Brushes.Black, FormRectangle)
        FormGraphics.DrawString(code, Font, Brushes.Gray, PosCode)
        Select Case change
            Case > 0
                FormGraphics.DrawString(FormatNumber(last, dec, vbTrue), Font, Brushes.Red, PosLast)
            Case = 0
                FormGraphics.DrawString(FormatNumber(last, dec, vbTrue), Font, Brushes.White, PosLast)
            Case < 0
                FormGraphics.DrawString(FormatNumber(last, dec, vbTrue), Font, Brushes.Green, PosLast)
        End Select
        Refresh()
    End Sub

    Private Sub QueryStocks()
        Do
            Dim stock As StockName = Stocks(StockIndex)
            Dim req As System.Net.HttpWebRequest = System.Net.WebRequest.Create("http://yunhq.sse.com.cn:32041/v1/" & stock.Market & "/list/self/" & stock.Code & "?select=last%2Cchange%2Cname")
            Dim reader As System.IO.StreamReader = New System.IO.StreamReader(req.GetResponse().GetResponseStream())
            Dim json As String = reader.ReadToEnd()
            Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer
            Dim res As Object = serializer.Deserialize(Of Object)(json)
            Dim last As Double = res("list")(0)(0)
            Dim change As Double = res("list")(0)(1)
            Dim name As String = res("list")(0)(2)
            Dim dec As Integer = 3
            If stock.Code(0) = "6" Or stock.Code(0) = "0" Or stock.Code(0) = "3" Then
                dec = 2
            End If
            Dim message As String = Format(StockIndex + 1) & "/" & Format(Stocks.Count) & " " & stock.Code
            Invoke(New Callback(AddressOf UpdateAfterQuery), message, last, change, dec)
        Loop
    End Sub

    Private Sub StockViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SoftWareKey = LocalMachine.OpenSubKey("SOFTWARE", True)
        StockViewerKey = SoftWareKey.OpenSubKey(Name, True)
        If StockViewerKey Is Nothing Then
            StockViewerKey = SoftWareKey.CreateSubKey(Name, True)
            CodesKey = StockViewerKey.OpenSubKey("codes", True)
            If CodesKey Is Nothing Then
                CodesKey = StockViewerKey.CreateSubKey("codes", True)
                CodesKey.SetValue("511380", "sh1")
            End If
        Else
            CodesKey = StockViewerKey.OpenSubKey("codes", True)
            If CodesKey Is Nothing Then
                CodesKey = StockViewerKey.CreateSubKey("codes", True)
                CodesKey.SetValue("511380", "sh1")
            End If
        End If
        Dim StocksNames As String() = CodesKey.GetValueNames()
        For i As Integer = 0 To CodesKey.ValueCount - 1
            Dim stock As StockName
            stock.Code = StocksNames(i)
            stock.Market = CodesKey.GetValue(StocksNames(i))
            Stocks.Add(stock)
        Next
        CodesKey.Close()
        StockViewerKey.Close()
        SoftWareKey.Close()

        ClientSize = New Size(128, 16)
        LabelClose.Location = New Point(112, 2)
        FormRectangle = New Rectangle(0, 0, ClientSize.Width, ClientSize.Height)
        PosCode = New Point(2, 2)
        PosLast = New Point(2 + 66, 2)
        FormBitmap = New Bitmap(ClientSize.Width, ClientSize.Height)
        FormGraphics = Graphics.FromImage(FormBitmap)
        QueryThread = New Threading.Thread(AddressOf QueryStocks)
        QueryThread.Start()
    End Sub

    Private Sub StockViewer_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        QueryThread.Abort()
    End Sub

    Private Sub StockViewer_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
        Opacity = FormOpacity
    End Sub

    Private Sub StockViewer_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        DragState = True
        DragPos = New Point(e.X, e.Y)
    End Sub

    Private Sub StockViewer_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        Opacity = 1
        If DragState Then
            MousePos = New Point(e.X, e.Y)
            Location = PointToScreen(MousePos) - DragPos
        End If
    End Sub

    Private Sub StockViewer_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        DragState = False
    End Sub

    Private Sub StockViewer_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
        If e.Delta > 0 Then
            FormOpacity += 0.05
            If FormOpacity > 1 Then
                FormOpacity = 1
            End If
        ElseIf e.Delta < 0 Then
            FormOpacity -= 0.05
            If FormOpacity < 0.05 Then
                FormOpacity = 0.05
            End If
        End If
    End Sub

    Private Sub StockViewer_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyCode
            Case Keys.Escape
                End
            Case Keys.Up
                StockIndex -= 1
                If StockIndex < 0 Then
                    StockIndex = Stocks.Count - 1
                End If
            Case Keys.Down
                StockIndex += 1
                If StockIndex >= Stocks.Count Then
                    StockIndex = 0
                End If
            Case Keys.Add, Keys.Oemplus
                If e.KeyCode = Keys.Add Or (e.KeyCode = Keys.Oemplus And e.Shift) Then
                    Dim NewCode As String = InputBox("Code:")
                    If NewCode <> "" Then
                        If Stocks.Count < 5 Then
                            Dim NewMarket As String = InputBox("Market(sh1/sz1):", , "sh1")
                            If NewMarket <> "" Then
                                Dim stock As StockName
                                stock.Code = NewCode
                                stock.Market = NewMarket
                                Stocks.Add(stock)
                                SoftWareKey = LocalMachine.OpenSubKey("SOFTWARE", True)
                                SoftWareKey.DeleteSubKeyTree(Name)
                                StockViewerKey = SoftWareKey.OpenSubKey(Name, True)
                                If StockViewerKey Is Nothing Then
                                    StockViewerKey = SoftWareKey.CreateSubKey(Name, True)
                                    CodesKey = StockViewerKey.OpenSubKey("codes", True)
                                    If CodesKey Is Nothing Then
                                        CodesKey = StockViewerKey.CreateSubKey("codes", True)
                                        For i As Integer = 0 To Stocks.Count - 1
                                            CodesKey.SetValue(Stocks(i).Code, Stocks(i).Market)
                                        Next
                                    End If
                                Else
                                    CodesKey = StockViewerKey.OpenSubKey("codes", True)
                                    If CodesKey Is Nothing Then
                                        CodesKey = StockViewerKey.CreateSubKey("codes", True)
                                        For i As Integer = 0 To Stocks.Count - 1
                                            CodesKey.SetValue(Stocks(i).Code, Stocks(i).Market)
                                        Next
                                    End If
                                End If
                                CodesKey.Close()
                                StockViewerKey.Close()
                                SoftWareKey.Close()
                            End If
                        End If
                    End If
                End If
            Case Keys.Subtract, Keys.OemMinus
                If e.KeyCode = Keys.Subtract Or (e.KeyCode = Keys.OemMinus And Not e.Shift) Then
                    Dim message As String = ""
                    For i As Integer = 0 To Stocks.Count - 1
                        message &= Str(i) & " " & Stocks(i).Code & vbCrLf
                    Next
                    Dim id As Integer = Int(InputBox("remove:" & vbCrLf & message, , Stocks.Count - 1))
                    If id >= 0 And id < Stocks.Count Then
                        Stocks.RemoveAt(id)
                        SoftWareKey = LocalMachine.OpenSubKey("SOFTWARE", True)
                        SoftWareKey.DeleteSubKeyTree(Name)
                        StockViewerKey = SoftWareKey.OpenSubKey(Name, True)
                        If StockViewerKey Is Nothing Then
                            StockViewerKey = SoftWareKey.CreateSubKey(Name, True)
                            CodesKey = StockViewerKey.OpenSubKey("codes", True)
                            If CodesKey Is Nothing Then
                                CodesKey = StockViewerKey.CreateSubKey("codes", True)
                                For i As Integer = 0 To Stocks.Count - 1
                                    CodesKey.SetValue(Stocks(i).Code, Stocks(i).Market)
                                Next
                            End If
                        Else
                            CodesKey = StockViewerKey.OpenSubKey("codes", True)
                            If CodesKey Is Nothing Then
                                CodesKey = StockViewerKey.CreateSubKey("codes", True)
                                For i As Integer = 0 To Stocks.Count - 1
                                    CodesKey.SetValue(Stocks(i).Code, Stocks(i).Market)
                                Next
                            End If
                        End If
                        CodesKey.Close()
                        StockViewerKey.Close()
                        SoftWareKey.Close()
                    End If
                End If
        End Select
    End Sub

    Private Sub StockViewer_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        e.Graphics.DrawImage(FormBitmap, 0, 0)
    End Sub

    Private Sub LabelClose_Click(sender As Object, e As EventArgs) Handles LabelClose.Click
        End
    End Sub

    Private Sub LabelClose_MouseMove(sender As Object, e As MouseEventArgs) Handles LabelClose.MouseMove
        Opacity = 1
        If Not LabelCloseState Then
            LabelClose.BackColor = Color.DarkGray
        End If
    End Sub

    Private Sub LabelClose_MouseLeave(sender As Object, e As EventArgs) Handles LabelClose.MouseLeave
        LabelClose.BackColor = Color.Gray
    End Sub

    Private Sub LabelClose_MouseDown(sender As Object, e As MouseEventArgs) Handles LabelClose.MouseDown
        LabelClose.BackColor = Color.DimGray
        LabelCloseState = True
    End Sub
End Class
