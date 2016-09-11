﻿Imports Pokemon3D.Screens.UI

Public Class PartyScreen

    Inherits Screen
    Implements ISelectionScreen

    Private Const POKEMON_TITLE As String = "Pokémon"

    Private _translation As Globalization.Classes.LOCAL_PartyScreen

    ''' <summary>
    ''' Cursor index -> pointing to Pokémon (0-5).
    ''' </summary>
    Private _index As Integer = 0

    Private _texture As Texture2D
    Private _menuTexture As Texture2D

    'Animation:
    Private _closing As Boolean = False

    Private _enrollY As Single = 0F
    Private _interfaceFade As Single = 0F
    Private _cursorPosition As New Vector2
    Private _cursorDest As New Vector2

    'Pokémon animation:
    Private Class PokemonAnimation
        Public _shakeV As Single
        Public _shakeLeft As Boolean
        Public _shakeCount As Integer
    End Class

    Private _pokemonAnimations As New List(Of PokemonAnimation)

    Private _menu As UI.SelectMenu

    Private _isSwitching As Boolean = False
    Private _switchIndex As Integer = -1

    'Message display:
    Private _messageDelay As Single = 0F
    Private _messageText As String = ""

    Public Sub New(ByVal currentScreen As Screen)
        Identification = Identifications.PartyScreen
        PreScreen = currentScreen
        IsDrawingGradients = True

        _translation = New Globalization.Classes.LOCAL_PartyScreen()

        _index = Core.Player.Temp.PokemonScreenIndex
        _texture = TextureManager.GetTexture("GUI\Menus\General")
        _menuTexture = TextureManager.GetTexture("GUI\Menus\PokemonInfo")

        _cursorDest = GetBoxPosition(_index)
        _cursorPosition = _cursorDest

        For i = 0 To Core.Player.Pokemons.Count - 1
            _pokemonAnimations.Add(New PokemonAnimation())
        Next

        CheckForLegendaryEmblem()
        CheckForOverkillEmblem()

        _menu = New UI.SelectMenu({""}.ToList(), 0, Nothing, 0)
        _menu.Visible = False
    End Sub

    Public Overrides Sub Draw()
        PreScreen.Draw()

        DrawGradients(CInt(255 * _interfaceFade))

        DrawBackground()
        DrawPokemonArea()

        If _messageDelay > 0F Then
            Dim textFade As Single = 1.0F
            If _messageDelay <= 1.0F Then
                textFade = _messageDelay
            End If

            Canvas.DrawRectangle(New Rectangle(CInt(Core.windowSize.Width / 2 - 150), CInt(Core.windowSize.Height - 200), 300, 100), New Color(0, 0, 0, CInt(150 * textFade * _interfaceFade)))

            Dim text As String = _messageText.Crop(FontManager.ChatFont, 250)
            Dim size As Vector2 = FontManager.ChatFont.MeasureString(text)

            SpriteBatch.DrawString(FontManager.ChatFont, text, New Vector2(CSng(Core.windowSize.Width / 2 - size.X / 2), CSng(Core.windowSize.Height - 150 - size.Y / 2)), New Color(255, 255, 255, CInt(255 * textFade * _interfaceFade)))
        End If
    End Sub

    Private Sub DrawBackground()
        Dim mainBackgroundColor As Color = Color.White
        If _closing Then
            mainBackgroundColor = New Color(255, 255, 255, CInt(255 * _interfaceFade))
        End If

        Dim halfWidth As Integer = CInt(Core.windowSize.Width / 2)
        Dim halfHeight As Integer = CInt(Core.windowSize.Height / 2)

        Canvas.DrawRectangle(New Rectangle(halfWidth - 400, halfHeight - 232, 260, 32), New Color(84, 198, 216, mainBackgroundColor.A))
        Canvas.DrawRectangle(New Rectangle(halfWidth - 140, halfHeight - 216, 16, 16), New Color(84, 198, 216, mainBackgroundColor.A))
        SpriteBatch.Draw(_menuTexture, New Rectangle(halfWidth - 140, halfHeight - 232, 16, 16), New Rectangle(32, 16, 16, 16), mainBackgroundColor)
        SpriteBatch.Draw(_menuTexture, New Rectangle(halfWidth - 124, halfHeight - 216, 16, 16), New Rectangle(32, 16, 16, 16), mainBackgroundColor)

        SpriteBatch.DrawString(FontManager.ChatFont, POKEMON_TITLE, New Vector2(halfWidth - 390, halfHeight - 228), mainBackgroundColor)

        For y = 0 To CInt(_enrollY) Step 16
            For x = 0 To 800 Step 16
                SpriteBatch.Draw(_menuTexture, New Rectangle(halfWidth - 400 + x, halfHeight - 200 + y, 16, 16), New Rectangle(0, 0, 4, 4), mainBackgroundColor)
            Next
        Next

        Dim modRes As Integer = CInt(_enrollY) Mod 16
        If modRes > 0 Then
            For x = 0 To 800 Step 16
                SpriteBatch.Draw(_menuTexture, New Rectangle(halfWidth - 400 + x, CInt(_enrollY + (halfHeight - 200)), 16, modRes), New Rectangle(0, 0, 4, 4), mainBackgroundColor)
            Next
        End If
    End Sub

    Private Sub DrawPokemonArea()
        For i = 0 To Core.Player.Pokemons.Count - 1
            DrawPokemon(i)
        Next

        Canvas.DrawBorder(3, New Rectangle(CInt(_cursorPosition.X) - 3, CInt(_cursorPosition.Y) - 3, 300, 82), New Color(200, 80, 80, CInt(200 * _interfaceFade)))

        If _isSwitching Then
            Dim switchPosition As Vector2 = GetBoxPosition(_switchIndex)

            Canvas.DrawBorder(3, New Rectangle(CInt(switchPosition.X) - 6, CInt(switchPosition.Y) - 6, 306, 88), New Color(80, 80, 200, CInt(200 * _interfaceFade)))
        End If
    End Sub

    Private Sub DrawPokemon(ByVal index As Integer)
        Dim position As Vector2 = GetBoxPosition(index)

        Dim p As Pokemon = Core.Player.Pokemons(index)

        Dim backColor As Color = New Color(0, 0, 0, CInt(100 * _interfaceFade))
        If p.IsShiny And p.IsEgg() = False Then
            backColor = New Color(57, 59, 29, CInt(100 * _interfaceFade))
        End If

        Canvas.DrawGradient(New Rectangle(CInt(position.X), CInt(position.Y), 32, 76), New Color(0, 0, 0, 0), backColor, True, -1)
        Canvas.DrawRectangle(New Rectangle(CInt(position.X) + 32, CInt(position.Y), 228, 76), backColor)
        Canvas.DrawGradient(New Rectangle(CInt(position.X) + 260, CInt(position.Y), 32, 76), backColor, New Color(0, 0, 0, 0), True, -1)

        If p.IsEgg() Then
            Dim percent As Integer = CInt((p.EggSteps / p.BaseEggSteps) * 100)
            Dim shakeMulti As Single = 1.0F
            If percent <= 33 Then
                shakeMulti = 0.2F
            ElseIf percent > 33 And percent <= 66 Then
                shakeMulti = 0.5F
            Else
                shakeMulti = 0.8F
            End If

            'menu image:
            SpriteBatch.Draw(p.GetMenuTexture(), New Rectangle(CInt(position.X) + 80 + 32, CInt(position.Y) + 6 + 32, 64, 64), Nothing, New Color(255, 255, 255, CInt(255 * _interfaceFade)),
                             _pokemonAnimations(index)._shakeV * shakeMulti, New Vector2(16, 16), SpriteEffects.None, 0F)

            'name:
            GetFontRenderer().DrawString(FontManager.MiniFont, p.GetDisplayName(), New Vector2(position.X + 156, position.Y + 27), New Color(255, 255, 255, CInt(255 * _interfaceFade)))
        Else
            Dim shakeMulti As Single = CSng((p.HP / p.MaxHP).Clamp(0.2F, 1.0F))

            'menu image:
            SpriteBatch.Draw(p.GetMenuTexture(), New Rectangle(CInt(position.X) + 2 + 32, CInt(position.Y) - 4 + 32, 64, 64), Nothing, New Color(255, 255, 255, CInt(255 * _interfaceFade)),
                             _pokemonAnimations(index)._shakeV * shakeMulti, New Vector2(16, 16), SpriteEffects.None, 0F)


            'Item:
            If p.Item IsNot Nothing Then
                SpriteBatch.Draw(p.Item.Texture, New Rectangle(CInt(position.X) + 42, CInt(position.Y) + 36, 24, 24), New Color(255, 255, 255, CInt(255 * _interfaceFade)))
            End If

            'name:
            GetFontRenderer().DrawString(FontManager.MiniFont, p.GetDisplayName(), New Vector2(position.X + 78, position.Y + 5), New Color(255, 255, 255, CInt(255 * _interfaceFade)))

            'Gender symbol:
            Select Case p.Gender
                Case Pokemon.Genders.Male
                    SpriteBatch.Draw(_menuTexture, New Rectangle(CInt(position.X + FontManager.MiniFont.MeasureString(p.GetDisplayName()).X + 86), CInt(position.Y + 9), 7, 13), New Rectangle(25, 0, 7, 13), New Color(255, 255, 255, CInt(255 * _interfaceFade)))
                Case Pokemon.Genders.Female
                    SpriteBatch.Draw(_menuTexture, New Rectangle(CInt(position.X + FontManager.MiniFont.MeasureString(p.GetDisplayName()).X + 85), CInt(position.Y + 9), 9, 13), New Rectangle(32, 0, 9, 13), New Color(255, 255, 255, CInt(255 * _interfaceFade)))
            End Select

            'Level:
            GetFontRenderer().DrawString(FontManager.MiniFont, _translation.LV_TEXT(p.Level.ToString()), New Vector2(position.X + 4, position.Y + 56), New Color(255, 255, 255, CInt(255 * _interfaceFade)))

            'HP Bar:
            SpriteBatch.Draw(_menuTexture, New Rectangle(CInt(position.X) + 78, CInt(position.Y) + 32, 135, 15), New Rectangle(0, 32, 90, 10), New Color(255, 255, 255, CInt(220 * _interfaceFade)))
            '108 pixels:
            With p
                Dim hpV As Double = .HP / .MaxHP
                Dim hpWidth As Integer = CInt((104 * _interfaceFade) * hpV)
                Dim hpColorX As Integer = 0
                If hpV < 0.5F Then
                    hpColorX = 5
                    If hpV < 0.1F Then
                        hpColorX = 10
                    End If
                End If
                If .HP > 0 And hpWidth = 0 Then
                    hpWidth = 1
                End If
                If hpWidth > 0 Then
                    Dim drawColor As Color = New Color(255, 255, 255, CInt(220 * _interfaceFade))

                    SpriteBatch.Draw(_menuTexture, New Rectangle(CInt(position.X) + 78 + 24, CInt(position.Y) + 35, 2, 8), New Rectangle(hpColorX, 42, 2, 6), drawColor)

                    SpriteBatch.Draw(_menuTexture, New Rectangle(CInt(position.X) + 78 + 24 + 2, CInt(position.Y) + 35, hpWidth, 8), New Rectangle(hpColorX + 2, 42, 1, 6), drawColor)

                    SpriteBatch.Draw(_menuTexture, New Rectangle(CInt(position.X) + 78 + 24 + 2 + hpWidth, CInt(position.Y) + 35, 2, 8), New Rectangle(hpColorX + 3, 42, 2, 6), drawColor)
                End If
            End With

            'HP display:
            GetFontRenderer().DrawString(FontManager.MiniFont, p.HP & " / " & p.MaxHP, New Vector2(position.X + 110, position.Y + 50), New Color(255, 255, 255, CInt(255 * _interfaceFade)))
        End If

        If _menu.Visible Then
            _menu.Draw()
        End If
    End Sub

    Protected Overrides Function GetFontRenderer() As SpriteBatch
        If IsCurrentScreen() And _interfaceFade + 0.01F >= 1.0F Then
            Return FontRenderer
        Else
            Return SpriteBatch
        End If
    End Function

    Private Function GetBoxPosition(ByVal index As Integer) As Vector2
        Dim position As New Vector2

        '292 x 76
        Dim halfWidth As Integer = CInt(Core.windowSize.Width / 2)
        Dim halfHeight As Integer = CInt(Core.windowSize.Height / 2)

        position.Y = CSng((Math.Floor(index / 2) * 128) + (halfHeight - 200) + 42)

        If index Mod 2 = 0 Then
            position.X = halfWidth - 328
        Else
            position.X = halfWidth + 36
        End If

        Return position
    End Function

    Public Overrides Sub Update()
        If _pokemonAnimations.Count > 0 Then
            Dim animation As PokemonAnimation = _pokemonAnimations(_index)
            If animation._shakeLeft Then
                animation._shakeV -= 0.035F
                If animation._shakeV <= -0.4F Then
                    animation._shakeCount -= 1
                    animation._shakeLeft = False
                End If
            Else
                animation._shakeV += 0.035F
                If animation._shakeV >= 0.4F Then
                    animation._shakeCount -= 1
                    animation._shakeLeft = True
                End If
            End If
        End If

        If _messageDelay > 0F Then
            _messageDelay -= 0.1F
            If _messageDelay <= 0F Then
                _messageDelay = 0F
            End If
        End If

        If _closing Then
            If _interfaceFade > 0F Then
                _interfaceFade = MathHelper.Lerp(0, _interfaceFade, 0.8F)
                If _interfaceFade < 0F Then
                    _interfaceFade = 0F
                End If
            End If
            If _enrollY > 0 Then
                _enrollY = MathHelper.Lerp(0, _enrollY, 0.8F)
                If _enrollY <= 0 Then
                    _enrollY = 0
                End If
            End If
            If _enrollY <= 2.0F Then
                SetScreen(PreScreen)
            End If
        Else
            Dim maxWindowHeight As Integer = 400
            If _enrollY < maxWindowHeight Then
                _enrollY = MathHelper.Lerp(maxWindowHeight, _enrollY, 0.8F)
                If _enrollY >= maxWindowHeight Then
                    _enrollY = maxWindowHeight
                End If
            End If
            If _interfaceFade < 1.0F Then
                _interfaceFade = MathHelper.Lerp(1, _interfaceFade, 0.95F)
                If _interfaceFade > 1.0F Then
                    _interfaceFade = 1.0F
                End If
            End If

            If _menu.Visible Then
                _menu.Update()
            Else
                If Controls.Down(True, True, False, True, True, True) And _index < Core.Player.Pokemons.Count - 2 Then
                    _index += 2
                    _cursorDest = GetBoxPosition(_index)
                End If
                If Controls.Up(True, True, False, True, True, True) And _index > 1 Then
                    _index -= 2
                    _cursorDest = GetBoxPosition(_index)
                End If
                If Controls.Left(True) And _index > 0 Then
                    _index -= 1
                    _cursorDest = GetBoxPosition(_index)
                End If
                If Controls.Right(True) And _index < Core.Player.Pokemons.Count - 1 Then
                    _index += 1
                    _cursorDest = GetBoxPosition(_index)
                End If

                Core.Player.Temp.PokemonScreenIndex = _index

                _cursorPosition.X = MathHelper.Lerp(_cursorDest.X, _cursorPosition.X, 0.8F)
                _cursorPosition.Y = MathHelper.Lerp(_cursorDest.Y, _cursorPosition.Y, 0.8F)

                If Controls.Accept() Then
                    If _isSwitching Then
                        _isSwitching = False

                        If _switchIndex <> _index Then
                            Dim p1 As Pokemon = Core.Player.Pokemons(_switchIndex)
                            Dim p2 As Pokemon = Core.Player.Pokemons(_index)

                            Core.Player.Pokemons(_switchIndex) = p2
                            Core.Player.Pokemons(_index) = p1
                        End If
                    Else
                        _cursorPosition = _cursorDest
                        CreateMainMenu()
                    End If
                End If

                If Controls.Dismiss() And CanExit Then
                    If _isSwitching Then
                        _isSwitching = False
                    Else
                        _closing = True
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub CreateMainMenu()
        If Mode = ISelectionScreen.ScreenMode.Default Then
            CreateNormalMenu(_translation.MENU_SUMMARY)
        ElseIf Mode = ISelectionScreen.ScreenMode.Selection
            CreateSelectionMenu()
        End If
    End Sub

    Private Sub CreateSelectionMenu()
        Dim items As New List(Of String)
        items.Add(_translation.MENU_SELECT)
        items.Add(_translation.MENU_SUMMARY)
        items.Add(_translation.MENU_BACK)

        _menu = New UI.SelectMenu(items, 0, AddressOf SelectSelectionMenuItem, items.Count - 1)
    End Sub

    Private Sub SelectSelectionMenuItem(ByVal selectMenu As UI.SelectMenu)
        Select Case selectMenu.SelectedItem
            Case _translation.MENU_SELECT
                'When a Pokémon got selected in Selection Mode, raise the selected event and close the screen.
                FireSelectionEvent(_index)
                _closing = True
            Case _translation.MENU_SUMMARY
                SetScreen(New SummaryScreen(Me, Core.Player.Pokemons.ToArray(), _index))
        End Select
    End Sub

    Private Sub CreateNormalMenu(ByVal selectedItem As String)
        Dim p As Pokemon = Core.Player.Pokemons(_index)

        Dim items As New List(Of String)
        items.Add(_translation.MENU_SUMMARY)

        If p.IsEgg() = False Then
            If CanUseMove(p, "Fly", Badge.HMMoves.Fly) Or
            CanUseMove(p, "Ride", Badge.HMMoves.Ride) Or
            CanUseMove(p, "Flash", Badge.HMMoves.Flash) Or
            CanUseMove(p, "Cut", Badge.HMMoves.Cut) Or
            CanUseMove(p, "Teleport", -1) Or
            CanUseMove(p, "Dig", -1) Then

                items.Add(_translation.MENU_FIELDMOVE)
            End If
        End If

        items.Add(_translation.MENU_SWITCH)

        If p.IsEgg() = False Then
            items.Add(_translation.MENU_ITEM)
        End If

        items.Add(_translation.MENU_BACK)

        _menu = New UI.SelectMenu(items, items.IndexOf(selectedItem), AddressOf SelectedMainMenuItem, items.Count - 1)
    End Sub

    Private Sub CreateFieldMoveMenu()
        Dim p As Pokemon = Core.Player.Pokemons(_index)

        Dim items As New List(Of String)
        If CanUseMove(p, "Fly", Badge.HMMoves.Fly) Then
            items.Add(_translation.MENU_FIELDMOVE_FLY)
        End If
        If CanUseMove(p, "Ride", Badge.HMMoves.Ride) Then
            items.Add(_translation.MENU_FIELDMOVE_RIDE)
        End If
        If CanUseMove(p, "Flash", Badge.HMMoves.Flash) Then
            items.Add(_translation.MENU_FIELDMOVE_FLASH)
        End If
        If CanUseMove(p, "Cut", Badge.HMMoves.Cut) Then
            items.Add(_translation.MENU_FIELDMOVE_CUT)
        End If
        If CanUseMove(p, "Teleport", -1) Then
            items.Add(_translation.MENU_FIELDMOVE_TELEPORT)
        End If
        If CanUseMove(p, "Dig", -1) Then
            items.Add(_translation.MENU_FIELDMOVE_DIG)
        End If

        items.Add(_translation.MENU_BACK)

        _menu = New UI.SelectMenu(items, 0, AddressOf SelectedFieldMoveMenuItem, items.Count - 1)
    End Sub

    Private Sub CreateItemMenu()
        Dim p As Pokemon = Core.Player.Pokemons(_index)

        Dim items As New List(Of String)

        items.Add(_translation.MENU_ITEM_GIVE)
        If p.Item IsNot Nothing Then
            items.Add(_translation.MENU_ITEM_TAKE)
        End If
        items.Add(_translation.MENU_BACK)

        _menu = New UI.SelectMenu(items, 0, AddressOf SelectedItemMenuItem, items.Count - 1)
    End Sub

    Private Function CanUseMove(ByVal p As Pokemon, ByVal moveName As String, ByVal hmMove As Integer) As Boolean
        If GameController.IS_DEBUG_ACTIVE Then
            Return True
        End If
        If p.IsEgg() = False Then
            If hmMove > -1 Then
                If Badge.CanUseHMMove(CType(hmMove, Badge.HMMoves)) = False Then
                    Return False
                End If

                For Each a As BattleSystem.Attack In p.Attacks
                    If a.Name.ToLower() = moveName.ToLower() Then
                        Return True
                    End If
                Next
            End If
        End If
        Return False
    End Function

    Private Function CanUseMove(ByVal p As Pokemon, ByVal moveName As String, ByVal hmMove As Badge.HMMoves) As Boolean
        Return CanUseMove(p, moveName, CInt(hmMove))
    End Function

    Private Sub SelectedMainMenuItem(ByVal selectMenu As UI.SelectMenu)
        Select Case selectMenu.SelectedItem
            Case _translation.MENU_SUMMARY
                SetScreen(New SummaryScreen(Me, Core.Player.Pokemons.ToArray(), _index))
            Case _translation.MENU_FIELDMOVE
                CreateFieldMoveMenu()
            Case _translation.MENU_SWITCH
                _switchIndex = _index
                _isSwitching = True
            Case _translation.MENU_ITEM
                CreateItemMenu()
        End Select
    End Sub

    Private Sub SelectedFieldMoveMenuItem(ByVal selectMenu As UI.SelectMenu)
        Select Case selectMenu.SelectedItem
            Case _translation.MENU_FIELDMOVE_FLY
                UseFly()
            Case _translation.MENU_FIELDMOVE_RIDE
                UseRide()
            Case _translation.MENU_FIELDMOVE_FLASH
                UseFlash()
            Case _translation.MENU_FIELDMOVE_CUT
                UseCut()
            Case _translation.MENU_FIELDMOVE_TELEPORT
                UseTeleport()
            Case _translation.MENU_FIELDMOVE_DIG
                UseDig()
            Case _translation.MENU_BACK
                CreateNormalMenu(_translation.MENU_FIELDMOVE)
        End Select
    End Sub

    Private Sub SelectedItemMenuItem(ByVal selectMenu As UI.SelectMenu)
        Select Case selectMenu.SelectedItem
            Case _translation.MENU_ITEM_GIVE
                Dim selScreen As New NewInventoryScreen(Core.CurrentScreen)
                selScreen.Mode = Screens.UI.ISelectionScreen.ScreenMode.Selection
                selScreen.CanExit = True

                AddHandler selScreen.SelectedObject, AddressOf GiveItemHandler

                Core.SetScreen(selScreen)
            Case _translation.MENU_ITEM_TAKE
                Dim p As Pokemon = Core.Player.Pokemons(_index)

                If p.Item.IsMail And p.Item.AdditionalData <> "" Then
                    ShowMessage(_translation.MESSAGE_MAILTAKEN)

                    Core.Player.Mails.Add(Items.MailItem.GetMailDataFromString(p.Item.AdditionalData))

                    p.Item = Nothing
                Else
                    ShowMessage(_translation.MESSAGE_ITEMTAKEN(p.Item.Name, p.GetDisplayName()))

                    Core.Player.Inventory.AddItem(p.Item.ID, 1)
                    p.Item = Nothing
                End If
            Case _translation.MENU_BACK
                CreateNormalMenu(_translation.MENU_ITEM)
        End Select
    End Sub

    ''' <summary>
    ''' A handler method to convert the incoming object array.
    ''' </summary>
    Private Sub GiveItemHandler(ByVal params As Object())
        GiveItem(CInt(params(0)))
    End Sub

    Private Sub GiveItem(ByVal itemID As Integer)
        Dim i As Item = Item.GetItemByID(itemID)

        If i.CanBeHold Then
            Dim p As Pokemon = Core.Player.Pokemons(_index)

            Core.Player.Inventory.RemoveItem(itemID, 1)

            Dim message As String = ""

            Dim reItem As Item = p.Item
            If reItem IsNot Nothing Then
                If reItem.IsMail And reItem.AdditionalData <> "" Then
                    Core.Player.Mails.Add(Items.MailItem.GetMailDataFromString(reItem.AdditionalData))

                    message = _translation.MESSAGE_SWITCH_ITEM_MAIL(i.Name, p.GetDisplayName())
                Else
                    Core.Player.Inventory.AddItem(reItem.ID, 1)

                    message = _translation.MESSAGE_SWITCH_ITEM(p.GetDisplayName(), i.Name, reItem.Name)
                End If
            Else
                message = _translation.MESSAGE_GIVE_ITEM(p.GetDisplayName(), i.Name)
            End If

            p.Item = i

            ShowMessage(message)
        Else
            ShowMessage(_translation.MESSAGE_GIVE_ITEM_ERROR(i.Name))
        End If
    End Sub

    Private Sub ShowMessage(ByVal text As String)
        _messageDelay = CSng(text.Length / 1.75)
        _messageText = text
    End Sub

    Public Overrides Sub SizeChanged()
        _cursorDest = GetBoxPosition(_index)
        _cursorPosition = _cursorDest
    End Sub

#Region "Emblems"

    Private Sub CheckForLegendaryEmblem()
        'This sub checks if Ho-Oh, Lugia and Suicune are in the player's party.
        Dim hasHoOh As Boolean = False
        Dim hasLugia As Boolean = False
        Dim hasSuicune As Boolean = False

        For Each p As Pokemon In Core.Player.Pokemons
            Select Case p.Number
                Case 245
                    hasSuicune = True
                Case 249
                    hasLugia = True
                Case 250
                    hasHoOh = True
            End Select
        Next

        If hasSuicune And hasLugia And hasHoOh Then
            GameJolt.Emblem.AchieveEmblem("legendary")
        End If
    End Sub

    Private Sub CheckForOverkillEmblem()
        If Core.Player.Pokemons.Count = 6 Then
            Dim has100 As Boolean = True
            For i = 0 To 5
                If Core.Player.Pokemons(i).Level < 100 Then
                    has100 = False
                    Exit For
                End If
            Next
            If has100 Then
                GameJolt.Emblem.AchieveEmblem("overkill")
            End If
        End If
    End Sub

#End Region

#Region "Field Moves"

    Private Sub UseFly()
        If Level.CanFly Or GameController.IS_DEBUG_ACTIVE Or Core.Player.SandBoxMode Then
            SetScreen(OverworldScreen.GetCurrentInstance())

            If Screen.Level.CurrentRegion.Contains(",") Then
                Dim regions As List(Of String) = Screen.Level.CurrentRegion.Split(CChar(",")).ToList()
                Core.SetScreen(New TransitionScreen(Core.CurrentScreen, New MapScreen(Core.CurrentScreen, regions, 0, {"Fly", Core.Player.Pokemons(_index)}), Color.White, False))
            Else
                Dim startRegion As String = Screen.Level.CurrentRegion
                Core.SetScreen(New TransitionScreen(Core.CurrentScreen, New MapScreen(Core.CurrentScreen, startRegion, {"Fly", Core.Player.Pokemons(_index)}), Color.White, False))
            End If
        Else
            ShowMessage(_translation.MESSAGE_FIELDMOVE_ERROR("Fly"))
        End If
    End Sub

    Private Sub UseFlash()
        SetScreen(OverworldScreen.GetCurrentInstance())

        If Screen.Level.IsDark Then
            Dim s As String = "@text.show(" & Core.Player.Pokemons(_index).GetDisplayName() & " used~Flash!)" & vbNewLine &
                              "@environment.toggledarkness" & vbNewLine &
                              "@sound.play(Battle\Effects\effect_thunderbolt)" & vbNewLine &
                              "@text.show(The area got lit up!)"
            PlayerStatistics.Track("Flash used", 1)
            Construct.Controller.GetInstance().RunFromString(s, {Construct.Controller.ScriptRunOptions.CheckDelay})
        Else
            Dim s As String = "@text.show(" & Core.Player.Pokemons(_index).GetDisplayName() & " used~Flash!)" & vbNewLine &
                                            "@sound.play(Battle\Effects\effect_thunderbolt)" & vbNewLine &
                                            "@text.show(The area is already~lit up!)"
            Construct.Controller.GetInstance().RunFromString(s, {Construct.Controller.ScriptRunOptions.CheckDelay})
        End If
    End Sub

    Private Sub UseRide()
        If Screen.Level.Riding Then
            Screen.Level.Riding = False
            Screen.Level.OwnPlayer.SetTexture(Core.Player.TempRideSkin, True)
            Core.Player.Skin = Core.Player.TempRideSkin

            Core.SetScreen(OverworldScreen.GetCurrentInstance())

            If Screen.Level.IsRadioOn = False OrElse GameJolt.PokegearScreen.StationCanPlay(Screen.Level.SelectedRadioStation) = False Then
                MusicPlayer.GetInstance().Play(Level.MusicLoop)
            End If
        Else
            If Screen.Level.Surfing = False And Screen.Camera.IsMoving() = False And Screen.Camera.Turning = False And Level.CanRide() Then
                Core.SetScreen(OverworldScreen.GetCurrentInstance())

                Screen.Level.Riding = True
                Core.Player.TempRideSkin = Core.Player.Skin

                Dim skin As String = "[POKEMON|"
                If Core.Player.Pokemons(_index).IsShiny Then
                    skin &= "S]"
                Else
                    skin &= "N]"
                End If
                skin &= Core.Player.Pokemons(_index).Number & PokemonForms.GetOverworldAddition(Core.Player.Pokemons(_index))

                Screen.Level.OwnPlayer.SetTexture(skin, False)

                SoundManager.PlayPokemonCry(Core.Player.Pokemons(_index).Number)

                TextBox.Show(Core.Player.Pokemons(_index).GetDisplayName() & " used~Ride!", {}, True, False)
                PlayerStatistics.Track("Ride used", 1)

                If Screen.Level.IsRadioOn = False OrElse GameJolt.PokegearScreen.StationCanPlay(Screen.Level.SelectedRadioStation) = False Then
                    MusicPlayer.GetInstance().Play("system\ride", True)
                End If
            Else
                ShowMessage(_translation.MESSAGE_FIELDMOVE_ERROR("Ride"))
            End If
        End If
    End Sub

    Private Sub UseCut()
        Dim grassEntities = Grass.GetGrassTilesAroundPlayer(2.4F)
        If grassEntities.Count > 0 Then
            Core.SetScreen(OverworldScreen.GetCurrentInstance())

            PlayerStatistics.Track("Cut used", 1)
            TextBox.Show(Core.Player.Pokemons(_index).GetDisplayName() & "~used Cut!", {}, True, False)
            Core.Player.Pokemons(_index).PlayCry()
            For Each e As Entity In grassEntities
                Screen.Level.Entities.Remove(e)
            Next
        Else
            ShowMessage(_translation.MESSAGE_FIELDMOVE_ERROR("Cut"))
        End If
    End Sub

    Private Sub UseTeleport()
        If Screen.Level.CanTeleport Or GameController.IS_DEBUG_ACTIVE Or Core.Player.SandBoxMode Then
            Core.SetScreen(OverworldScreen.GetCurrentInstance())

            Dim setToFirstPerson As Boolean = Not CType(Screen.Camera, OverworldCamera).ThirdPerson

            Dim yFinish As String = (Screen.Camera.Position.Y + 2.9F).ToString().ReplaceDecSeparator()

            Dim s As String = "@text.show(" & Core.Player.Pokemons(_index).GetDisplayName() & "~used Teleport!)
@level.wait(20)
@camera.activatethirdperson
@camera.reset
@camera.fix
@player.turnto(0)
@sound.play(teleport)
:while:<player.position(y)><" & yFinish & "
@player.turn(1)
@player.warp(~,~+0.1,~)
@level.wait(1)
:endwhile
@screen.fadeout
@camera.defix
@player.warp(" & Core.Player.LastRestPlace & "," & Core.Player.LastRestPlacePosition & ",0)
@player.turnto(2)"

            If setToFirstPerson Then
                s &= vbNewLine & "@camera.deactivatethirdperson"
            End If
            s &= vbNewLine &
"@level.update
@screen.fadein"

            PlayerStatistics.Track("Teleport used", 1)
            Construct.Controller.GetInstance().RunFromString(s, {Construct.Controller.ScriptRunOptions.CheckDelay})
        Else
            ShowMessage(_translation.MESSAGE_FIELDMOVE_ERROR("Teleport"))
        End If
    End Sub

    Private Sub UseDig()
        If Screen.Level.CanDig Or GameController.IS_DEBUG_ACTIVE Or Core.Player.SandBoxMode Then
            Core.SetScreen(OverworldScreen.GetCurrentInstance())

            Dim setToFirstPerson As Boolean = Not CType(Screen.Camera, OverworldCamera).ThirdPerson

            Dim s As String = "@text.show(" & Core.Player.Pokemons(_index).GetDisplayName() & " used Dig!)
@level.wait(20)
@camera.activatethirdperson
@camera.reset
@camera.fix
@player.turnto(0)
@sound.play(destroy)
:while:<player.position(y)>>" & (Screen.Camera.Position.Y - 1.4).ToString().ReplaceDecSeparator() & "
@player.turn(1)
@player.warp(~,~-0.1,~)
@level.wait(1)
:endwhile
@screen.fadeout
@camera.defix
@player.warp(" & Core.Player.LastRestPlace & "," & Core.Player.LastRestPlacePosition & ",0)" & vbNewLine &
"@player.turnto(2)"

            If setToFirstPerson Then
                s &= vbNewLine & "@camera.deactivatethirdperson"
            End If
            s &= vbNewLine &
"@level.update
@screen.fadein"

            PlayerStatistics.Track("Dig used", 1)
            Construct.Controller.GetInstance().RunFromString(s, {Construct.Controller.ScriptRunOptions.CheckDelay})
        Else
            ShowMessage(_translation.MESSAGE_FIELDMOVE_ERROR("Dig"))
        End If
    End Sub

#End Region

    Private _mode As ISelectionScreen.ScreenMode = ISelectionScreen.ScreenMode.Default
    Private _canExit As Boolean = True

    Public Event SelectedObject(params() As Object) Implements ISelectionScreen.SelectedObject

    Private Sub FireSelectionEvent(ByVal pokemonIndex As Integer)
        RaiseEvent SelectedObject(New Object() {pokemonIndex})
    End Sub

    ''' <summary>
    ''' The current mode of this screen.
    ''' </summary>
    Public Property Mode As ISelectionScreen.ScreenMode Implements ISelectionScreen.Mode
        Get
            Return _mode
        End Get
        Set(value As ISelectionScreen.ScreenMode)
            _mode = value
        End Set
    End Property

    ''' <summary>
    ''' If the user can quit the screen in selection mode without choosing an item.
    ''' </summary>
    Public Property CanExit As Boolean Implements ISelectionScreen.CanExit
        Get
            Return _canExit
        End Get
        Set(value As Boolean)
            _canExit = value
        End Set
    End Property

End Class