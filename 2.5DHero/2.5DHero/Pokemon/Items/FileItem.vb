﻿Namespace Items

    ''' <summary>
    ''' This class enables content creators to create own items with own images, functions and stats.
    ''' </summary>
    Public Class FileItem

        Inherits Item

        Private _isValid As Boolean = True
        Private _scriptBinding As String = ""

        Private _textureSource As String = "Items\ItemSheet"

        ''' <summary>
        ''' Returns the item of the FileItem class.
        ''' </summary>
        Public ReadOnly Property Item() As Item
            Get
                Return Me
            End Get
        End Property

        ''' <summary>
        ''' Returns if the FileItem is generated in a valid state.
        ''' </summary>
        Public ReadOnly Property IsValid() As Boolean
            Get
                Return _isValid
            End Get
        End Property

        ''' <summary>
        ''' This is the script binding that represents the script that gets executed once the item gets used.
        ''' </summary>
        Public Property ScriptBinding() As String
            Get
                Return _scriptBinding
            End Get
            Set(value As String)
                _scriptBinding = value
            End Set
        End Property

        ''' <summary>
        ''' Iniztializes a new FileItem class.
        ''' </summary>
        Public Sub New(ByVal data As String)
            data = data.Remove(0, 1)
            data = data.Remove(data.Length - 1, 1)

            Dim dataArr() As String = data.Split(CChar("|"))

            Dim ID As Integer = 0
            Dim Name As String = ""
            Dim Price As Integer = 0
            Dim ItemType As ItemTypes = ItemTypes.Standard
            Dim CatchMultiplier As Single = 0.0F
            Dim SortValue As Integer = 0
            Dim Description As String = ""
            Dim TextureRectangle As Rectangle = Nothing
            Dim TextureSource As String = ""
            Dim ScriptBinding As String = ""

            If dataArr.Count = 11 Then
                If IsNumeric(dataArr(0)) = True Then
                    ID = CInt(dataArr(0))
                Else
                    Logger.Log("220", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""ID"" did not have the correct data type (int).")
                    _isValid = False
                End If

                If dataArr(1) <> "" Then
                    Name = dataArr(1)
                Else
                    Logger.Log("221", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""Name"" cannot be empty.")
                    _isValid = False
                End If

                If IsNumeric(dataArr(2)) = True Then
                    Price = CInt(dataArr(2))
                Else
                    Logger.Log("222", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""Price"" did not have the correct data type (int).")
                    _isValid = False
                End If

                Select Case dataArr(3).ToLower()
                    Case "standard"
                        ItemType = ItemTypes.Standard
                    Case "medicine"
                        ItemType = ItemTypes.Medicine
                    Case "plants"
                        ItemType = ItemTypes.Plants
                    Case "pokeballs", "pokéballs"
                        ItemType = ItemTypes.Pokéballs
                    Case "machines"
                        ItemType = ItemTypes.Machines
                    Case "keyitems", "keyitem"
                        ItemType = ItemTypes.KeyItems
                    Case "mail"
                        ItemType = ItemTypes.Mail
                    Case "battleitems"
                        ItemType = ItemTypes.BattleItems
                    Case Else
                        Logger.Log("223", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""ItemType"" did not contain an ItemType data type.")
                        _isValid = False
                End Select

                If IsNumeric(dataArr(4).Replace(".", GameController.DecSeparator)) = True Then
                    CatchMultiplier = CSng(dataArr(4).Replace(".", GameController.DecSeparator))
                Else
                    Logger.Log("224", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""CatchMultiplier"" did not have the correct data type (sng).")
                    _isValid = False
                End If

                If IsNumeric(dataArr(5)) = True Then
                    SortValue = CInt(dataArr(5))
                Else
                    Logger.Log("225", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""SortValue"" did not have the correct data type (int).")
                    _isValid = False
                End If

                If dataArr(6) <> "" Then
                    Description = dataArr(6)
                Else
                    Logger.Log("226", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""Description"" cannot be empty.")
                    _isValid = False
                End If

                If dataArr(7).CountSeperators(",") = 3 Then
                    Dim texData() As String = dataArr(7).Split(CChar(","))
                    If texData.ToList().IsNumericList() = True Then
                        TextureRectangle = New Rectangle(CInt(texData(0)), CInt(texData(1)), CInt(texData(2)), CInt(texData(3)))
                    Else
                        Logger.Log("227", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""TextureRectangle"" contains invalid data.")
                        _isValid = False
                    End If
                Else
                    Logger.Log("228", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""TextureRectangle"" contains invalid data.")
                    _isValid = False
                End If

                TextureSource = dataArr(8)

                If dataArr(9) <> "" Then
                    ScriptBinding = dataArr(9)
                Else
                    Logger.Log("229", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""ScriptBinding"" cannot be empty.")
                    _isValid = False
                End If

                If dataArr(10).CountSeperators(",") = 3 Then
                    Dim attData() As String = dataArr(10).Split(CChar(","))
                    If attData.ToList().IsBooleanicList() = True Then
                        _canBeUsed = CBool(attData(0))
                        _canBeTraded = CBool(attData(1))
                        _canBeHold = CBool(attData(2))
                        _isBall = CBool(attData(3))
                        If _isBall = True Then
                            _canBeUsedInBattle = True
                        End If
                    Else
                        Logger.Log("230", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""ItemAttributes"" contains invalid data.")
                        _isValid = False
                    End If
                Else
                    Logger.Log("231", Logger.LogTypes.ErrorMessage, "FileItem.vb: The field for ""ItemAttributes"" contains invalid data.")
                    _isValid = False
                End If
            Else
                Logger.Log("232", Logger.LogTypes.ErrorMessage, "FileItem.vb: The array length (" & dataArr.Count & ") was smaller or greater than expected (11).")
                _isValid = False
            End If

            If _isValid = True Then
                Initialize(Name, Price, ItemType, ID, CatchMultiplier, SortValue, TextureRectangle, Description)
                _scriptBinding = ScriptBinding

                If TextureSource <> "" Then
                    _textureSource = TextureSource
                End If
            End If
        End Sub

        Protected Overrides Sub LoadTexture()
            _texture = TextureManager.GetTexture(_textureSource, _textureRectangle, "")
        End Sub

        Public Overrides Sub Use()
            Dim s As Screen = CurrentScreen
            While s.Identification <> Screen.Identifications.OverworldScreen And Not s.PreScreen Is Nothing
                s = s.PreScreen
            End While

            If s.Identification = Screen.Identifications.OverworldScreen Then
                SetScreen(s)
                Construct.Controller.GetInstance().RunFromFile(_scriptBinding, {Construct.Controller.ScriptRunOptions.CheckDelay})
            Else
                Logger.Log("233", Logger.LogTypes.Warning, "FileItem.vb: The Item """ & Name & """ created by the GameMode """ & GameModeManager.ActiveGameMode.Name & """ was used in a bad environment.")
            End If
        End Sub

    End Class

End Namespace