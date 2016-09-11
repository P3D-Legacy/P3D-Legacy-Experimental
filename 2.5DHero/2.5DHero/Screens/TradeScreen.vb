﻿Imports Pokemon3D.Construct.Framework

Public Class TradeScreen

    Inherits Screen

#Region "Navigation"

    Private Enum MenuStates
        MainPage
        BuyItems
        SellItems
        BuyItemsCategory
        SellItemsCategory
        SellItemsConfirmation
    End Enum

    Private MenuState As MenuStates = MenuStates.MainPage
    Private CurrentCategory As Item.ItemTypes = Item.ItemTypes.Medicine

    Private MainCursor As Integer = 0
    Private CategoryCursor As Integer = 0
    Private BuySellCursor As Integer = 0

    Private BuySellScroll As Integer = 0
    Private CategoryScroll As Integer = 0

    Private Property Scroll() As Integer
        Get
            Select Case MenuState
                Case MenuStates.MainPage
                    Return 0
                Case MenuStates.BuyItems, MenuStates.SellItems, MenuStates.SellItemsConfirmation
                    Return BuySellScroll
                Case MenuStates.BuyItemsCategory, MenuStates.SellItemsCategory
                    Return CategoryScroll
            End Select
            Return 0
        End Get
        Set(value As Integer)
            Select Case MenuState
                Case MenuStates.BuyItems, MenuStates.SellItems, MenuStates.SellItemsConfirmation
                    BuySellScroll = value
                Case MenuStates.BuyItemsCategory, MenuStates.SellItemsCategory
                    CategoryScroll = value
            End Select
        End Set
    End Property

    Private Property Cursor() As Integer
        Get
            Select Case MenuState
                Case MenuStates.MainPage
                    Return MainCursor
                Case MenuStates.BuyItems, MenuStates.SellItems, MenuStates.SellItemsConfirmation
                    Return BuySellCursor
                Case MenuStates.BuyItemsCategory, MenuStates.SellItemsCategory
                    Return CategoryCursor
            End Select

            Return 0
        End Get
        Set(value As Integer)
            Select Case MenuState
                Case MenuStates.MainPage
                    MainCursor = value
                Case MenuStates.BuyItems, MenuStates.SellItems, MenuStates.SellItemsConfirmation
                    BuySellCursor = value
                Case MenuStates.BuyItemsCategory, MenuStates.SellItemsCategory
                    CategoryCursor = value
            End Select
        End Set
    End Property

#End Region

    Public Enum Currencies
        Pokédollar
        BattlePoints
    End Enum

    ''' <summary>
    ''' Contains definitions for an item that can be traded in a store.
    ''' </summary>
        Public Structure TradeItem

        ''' <summary>
        ''' Creates a new instance of the TradeItem structure.
        ''' </summary>
        ''' <param name="ItemID">The ID of the Item.</param>
        ''' <param name="Price">The Price of the Item. Leave -1 for automatic Price.</param>
        ''' <param name="Amount">The Amount of the Item available in the store. Leave -1 for infinite.</param>
                Public Sub New(ByVal ItemID As Integer, ByVal Amount As Integer, ByVal Price As Integer, ByVal Currency As Currencies)
            Me.ItemID = ItemID
            Me.Amount = Amount

            If Price = -1 Then
                Dim i As Item = GetItem()

                If Currency = Currencies.BattlePoints Then
                    Me.Price = i.BattlePointsPrice
                ElseIf Currency = Currencies.Pokédollar Then
                    Me.Price = i.PokéDollarPrice
                End If
            Else
                Me.Price = Price
            End If
        End Sub

        Public ItemID As Integer
        Public Price As Integer
        Public Amount As Integer

        ''' <summary>
        ''' Returns the Item that is associated with this TradeItem instance.
        ''' </summary>
        ''' <returns></returns>
                Public Function GetItem() As Item
            Return Item.GetItemByID(ItemID)
        End Function

        Public Function SellPrice() As Integer
            Return CInt(Math.Ceiling(Price / 2))
        End Function

    End Structure

    Private TradeItems As New List(Of TradeItem)
    Private CanBuyItems As Boolean = True
    Private CanSellItems As Boolean = True
    Private Currency As Currencies = Currencies.Pokédollar

    Dim texture As Texture2D
    Dim TileOffset As Integer = 0
    Dim Title As String = ""

    ''' <summary>
    ''' Creates a new instance of the TradeScreen class.
    ''' </summary>
    ''' <param name="currentScreen">The screen that is the current active screen.</param>
    ''' <param name="storeString">The string defining what the store sells. Format: {ItemID|Amount|Price}</param>
        Public Sub New(ByVal currentScreen As Screen, ByVal storeString As String, ByVal canBuy As Boolean, ByVal canSell As Boolean, ByVal currencyIndicator As String)
        PreScreen = currentScreen

        Dim itemArr = storeString.Split({"}"}, StringSplitOptions.RemoveEmptyEntries)

        SetCurrency(currencyIndicator)

        For Each lItem As String In itemArr
            lItem = lItem.Remove(0, 1)

            Dim itemData = lItem.Split(CChar("|"))

            TradeItems.Add(New TradeItem(Converter.ToInteger(itemData(0)), Converter.ToInteger(itemData(1)), Converter.ToInteger(itemData(2)), Currency))
        Next

        texture = TextureManager.GetTexture("GUI\Menus\General")

        MouseVisible = True
        CanMuteMusic = True
        CanBePaused = True

        CanBuyItems = canBuy
        CanSellItems = canSell

        Title = "Store"

        CreateMainMenuButtons()
    End Sub

    Private Sub CreateMainMenuButtons()
        If mainMenuButtons.Count = 0 Then
            If CanBuyItems = True Then
                mainMenuButtons.Add("Buy")
            End If
            If CanSellItems = True Then
                mainMenuButtons.Add("Sell")
            End If
            mainMenuButtons.Add("Exit")
        End If
    End Sub

    ''' <summary>
    ''' Sets the currency from the currency indicator string.
    ''' </summary>
    ''' <param name="currencyIndicator">The currency indicator.</param>
        Private Sub SetCurrency(ByVal currencyIndicator As String)
        Select Case currencyIndicator.ToLower()
            Case "p", "pokedollar", "pokédollar", "poke", "poké", "poke dollar", "poké dollar", "money"
                Currency = Currencies.Pokédollar
            Case "bp", "battlepoints", "battle points"
                Currency = Currencies.BattlePoints
        End Select
    End Sub

    Public Overrides Sub Update()
        Select Case MenuState
            Case MenuStates.MainPage
                UpdateMain()
            Case MenuStates.BuyItemsCategory
                UpdateBuyCategory()
            Case MenuStates.BuyItems
                UpdateBuyItems()
            Case MenuStates.SellItemsCategory
                UpdateSellCategory()
            Case MenuStates.SellItems
                UpdateSellItems()
            Case MenuStates.SellItemsConfirmation
                UpdateSellConfirmation()
        End Select

        TileOffset += 1
        If TileOffset >= 64 Then
            TileOffset = 0
        End If
    End Sub

    Public Overrides Sub Draw()
        Canvas.DrawRectangle(windowSize, New Color(84, 198, 216))

        For y = -64 To windowSize.Height Step 64
            SpriteBatch.Draw(texture, New Rectangle(windowSize.Width - 128, y + TileOffset, 128, 64), New Rectangle(48, 0, 16, 16), Color.White)
        Next

        Canvas.DrawGradient(New Rectangle(0, 0, CInt(windowSize.Width), 200), New Color(42, 167, 198), New Color(42, 167, 198, 0), False, -1)
        Canvas.DrawGradient(New Rectangle(0, CInt(windowSize.Height - 200), CInt(windowSize.Width), 200), New Color(42, 167, 198, 0), New Color(42, 167, 198), False, -1)

        SpriteBatch.DrawString(FontManager.MainFont, Title, New Vector2(100, 24), Color.White, 0.0F, Vector2.Zero, 2.0F, SpriteEffects.None, 0.0F)

        Select Case MenuState
            Case MenuStates.MainPage
                DrawMain()
            Case MenuStates.BuyItemsCategory
                DrawBuyCategory()
            Case MenuStates.BuyItems
                DrawBuyItems()
            Case MenuStates.SellItemsCategory
                DrawSellCategory()
            Case MenuStates.SellItems
                DrawSellItems()
            Case MenuStates.SellItemsConfirmation
                DrawSellConfirmation()
        End Select
    End Sub

#Region "Mainscreen"

    Dim mainMenuButtons As New List(Of String)

    ''' <summary>
    ''' Updates the main screen.
    ''' </summary>
        Private Sub UpdateMain()
        Title = "Store"

        If Controls.Up(True, True, True, True, True, True) = True Then
            Cursor -= 1
            If Controls.ShiftDown() = True Then
                Cursor -= 4
            End If
        End If
        If Controls.Down(True, True, True, True, True, True) = True Then
            Cursor += 1
            If Controls.ShiftDown() = True Then
                Cursor += 4
            End If
        End If
        Cursor = Cursor.Clamp(0, mainMenuButtons.Count - 1)

        If Controls.Accept(True, False, False) = True Then
            For i = 0 To mainMenuButtons.Count - 1
                If New Rectangle(100, 100 + i * 96, 64 * 7, 64).Contains(MouseHandler.MousePosition) = True Then
                    If i = Cursor Then
                        ClickMainButton()
                    Else
                        Cursor = i
                    End If
                End If
            Next
        End If

        If Controls.Accept(False, True, True) = True Then
            ClickMainButton()
        End If

        If Controls.Dismiss(True, True, True) = True Then
            ButtonMainExit()
        End If
    End Sub

    Private Sub ClickMainButton()
        Select Case mainMenuButtons(Cursor)
            Case "Buy"
                ButtonMainBuy()
            Case "Sell"
                ButtonMainSell()
            Case "Exit"
                ButtonMainExit()
        End Select
    End Sub

    ''' <summary>
    ''' Event when clicking on the Buy button.
    ''' </summary>
        Private Sub ButtonMainBuy()
        MenuState = MenuStates.BuyItemsCategory
        Cursor = 0
        Scroll = 0
        LoadBuyCategoriesItems()
    End Sub

    ''' <summary>
    ''' Event when clicking on the Sell button.
    ''' </summary>
        Private Sub ButtonMainSell()
        MenuState = MenuStates.SellItemsCategory
        Cursor = 0
        Scroll = 0
        LoadSellCategoryItems()
    End Sub

    ''' <summary>
    ''' Event when clicking on the Exit button.
    ''' </summary>
        Private Sub ButtonMainExit()
        SetScreen(New TransitionScreen(CurrentScreen, PreScreen, Color.White, False))
    End Sub

    ''' <summary>
    ''' Draw the main screen.
    ''' </summary>
        Private Sub DrawMain()
        Dim y As Integer = 100
        For Each b As String In mainMenuButtons
            DrawButton(New Vector2(100, y), 5, b, 16)
            y += 96
        Next

        DrawMainCursor()
    End Sub

    ''' <summary>
    ''' Draw the cursor on the main screen.
    ''' </summary>
        Private Sub DrawMainCursor()
        Dim cPosition As Vector2 = New Vector2(380, 100 + Cursor * 96 - 42)

        Dim t As Texture2D = TextureManager.GetTexture("GUI\Menus\General", New Rectangle(0, 0, 16, 16), "")
        SpriteBatch.Draw(t, New Rectangle(CInt(cPosition.X), CInt(cPosition.Y), 64, 64), Color.White)
    End Sub

#End Region

#Region "BuyCategoryScreen"

    Private loadedBuyCategories As New List(Of Item.ItemTypes)

    Private Sub LoadBuyCategoriesItems()
        loadedBuyCategories.Clear()
        For Each i As TradeItem In TradeItems
            Dim item As Item = i.GetItem()

            If loadedBuyCategories.Contains(item.ItemType) = False And item.CanBeTraded = True Then
                loadedBuyCategories.Add(item.ItemType)
            End If
        Next
        loadedBuyCategories = (From c As Item.ItemTypes In loadedBuyCategories Order By CInt(c)).ToList()
    End Sub

    Private Sub UpdateBuyCategory()
        Title = "Buy Items"

        If loadedBuyCategories.Count > 0 Then
            If Controls.Down(True, True, True, True, True, True) = True Then
                Cursor += 1
                If Controls.ShiftDown() = True Then
                    Cursor += 4
                End If
            End If
            If Controls.Up(True, True, True, True, True, True) = True Then
                Cursor -= 1
                If Controls.ShiftDown() = True Then
                    Cursor -= 4
                End If
            End If

            While Cursor > 5
                Cursor -= 1
                Scroll += 1
            End While
            While Cursor < 0
                Cursor += 1
                Scroll -= 1
            End While

            If loadedBuyCategories.Count < 7 Then
                Scroll = 0
            Else
                Scroll = Scroll.Clamp(0, loadedBuyCategories.Count - 6)
            End If
            If loadedBuyCategories.Count < 6 Then
                Cursor = Cursor.Clamp(0, loadedBuyCategories.Count - 1)
            Else
                Cursor = Cursor.Clamp(0, 5)
            End If

            If Controls.Accept(True, False, False) = True Then
                For i = Scroll To Scroll + 5
                    If i <= loadedBuyCategories.Count - 1 Then
                        If New Rectangle(100, 100 + (i - Scroll) * 96, 64 * 7, 64).Contains(MouseHandler.MousePosition) = True Then
                            If i = Scroll + Cursor Then
                                ButtonBuyCategoriesAccept()
                            Else
                                Cursor = i - Scroll
                            End If
                        End If
                    End If
                Next
            End If

            If Controls.Accept(False, True, True) = True Then
                ButtonBuyCategoriesAccept()
            End If
        End If

        If Controls.Dismiss(True, True, True) = True Then
            MenuState = MenuStates.MainPage
        End If
    End Sub

    Private Sub ButtonBuyCategoriesAccept()
        CurrentCategory = loadedBuyCategories(Cursor + Scroll)
        MenuState = MenuStates.BuyItems
        Cursor = 0
        Scroll = 0
        LoadBuyItemsList()
    End Sub

    Private Sub DrawBuyCategory()
        If loadedBuyCategories.Count > 0 Then
            For i = Scroll To Scroll + 5
                If i <= loadedBuyCategories.Count - 1 Then
                    Dim p As Integer = i - Scroll

                    DrawButton(New Vector2(100, 100 + p * 96), 5, Item.GetItemTypeName(loadedBuyCategories(i)), 16, GetItemTypeTexture(loadedBuyCategories(i)))
                End If
            Next

            Canvas.DrawRectangle(New Rectangle(580, 100, 240, 48), New Color(255, 255, 255, 127))

            If loadedBuyCategories.Count > 0 Then
                Dim x As Integer = 0
                Dim y As Integer = 0

                For Each i As TradeItem In TradeItems
                    Dim item As Item = i.GetItem()
                    If item.ItemType = loadedBuyCategories(Cursor + Scroll) Then
                        SpriteBatch.Draw(item.Texture, New Rectangle(580 + x * 48, 100 + y * 48, 48, 48), Color.White)

                        x += 1
                        If x = 5 Then
                            x = 0
                            y += 1
                            Canvas.DrawRectangle(New Rectangle(580, 100 + y * 48, 240, 48), New Color(255, 255, 255, 127))
                        End If
                    End If
                Next
            End If

            DrawMainCursor()
        Else
            DrawBanner(New Vector2(CSng(windowSize.Width / 2 - 250), CSng(windowSize.Height / 2 - 50)), 100, "There are no items to buy.", FontManager.MainFont, 500)
        End If
    End Sub

#End Region

#Region "BuyItemsScreen"

    Private BuySellSparkleRotation As Single = 0.0F
    Private BuySellItemSize As Single = 192.0F
    Private BuySellItemShrinking As Boolean = True

    Private BuyItemsList As New List(Of TradeItem)
    Private BuyItemsAmount As Integer = 1
    Private BuyItemsShowDescription As Boolean = False

    Private Sub LoadBuyItemsList()
        BuyItemsList.Clear()
        For Each i As TradeItem In TradeItems
            Dim item As Item = i.GetItem()
            If item.ItemType = CurrentCategory Then
                BuyItemsList.Add(i)
            End If
        Next
        BuyItemsList = (From i As TradeItem In BuyItemsList Order By i.GetItem().Name).ToList()
    End Sub

    Private Sub UpdateBuyItems()
        Title = "Buy " & Item.GetItemTypeName(CurrentCategory)

        If Controls.Down(True, True, True, True, True, True) = True Then
            Cursor += 1
            If Controls.ShiftDown() = True Then
                Cursor += 4
            End If
        End If
        If Controls.Up(True, True, True, True, True, True) = True Then
            Cursor -= 1
            If Controls.ShiftDown() = True Then
                Cursor -= 4
            End If
        End If
        If Controls.Right(True, True, False, True, True, True) = True Then
            BuyItemsAmount += 1
            If Controls.ShiftDown() = True Then
                BuyItemsAmount += 4
            End If
        End If
        If Controls.Left(True, True, False, True, True, True) = True Then
            BuyItemsAmount -= 1
            If Controls.ShiftDown() = True Then
                BuyItemsAmount -= 4
            End If
        End If

        While Cursor > 5
            Cursor -= 1
            Scroll += 1
        End While
        While Cursor < 0
            Cursor += 1
            Scroll -= 1
        End While

        If BuyItemsList.Count < 7 Then
            Scroll = 0
        Else
            Scroll = Scroll.Clamp(0, BuyItemsList.Count - 6)
        End If
        If BuyItemsList.Count < 6 Then
            Cursor = Cursor.Clamp(0, BuyItemsList.Count - 1)
        Else
            Cursor = Cursor.Clamp(0, 5)
        End If

        If Controls.Accept(True, False, False) = True Then
            For i = Scroll To Scroll + 5
                If i <= BuyItemsList.Count - 1 Then
                    If New Rectangle(100, 100 + (i - Scroll) * 96, 64 * 7, 64).Contains(MouseHandler.MousePosition) = True Then
                        Cursor = i - Scroll
                    End If
                End If
            Next

            'Item Description:
            If New Rectangle(736, 160, 256, 256).Contains(MouseHandler.MousePosition) = True Then
                BuyItemsShowDescription = Not BuyItemsShowDescription
            End If

            '- button:
            If New Rectangle(664, 484, 64, 64).Contains(MouseHandler.MousePosition) = True Then
                ButtonBuyItemsMinus()
            End If
            '+ button:
            If New Rectangle(856, 484, 64, 64).Contains(MouseHandler.MousePosition) = True Then
                ButtonBuyItemsPlus()
            End If

            'Buy button:
            If New Rectangle(664 + 64, 484 + 64 + 22, 64 * 3, 64).Contains(MouseHandler.MousePosition) = True Then
                ButtonBuyItemsBuy()
            End If
        End If

        If ControllerHandler.ButtonPressed(Buttons.Y) = True Or KeyBoardHandler.KeyPressed(KeyBindings.SpecialKey) = True Then
            BuyItemsShowDescription = Not BuyItemsShowDescription
        End If

        If BuyItemsList.Count > 0 Then
            BuyItemsAmount = BuyItemsAmount.Clamp(0, GetMaxBuyItemAmount(BuyItemsList(Scroll + Cursor)))
        End If

        If Controls.Accept(False, True, True) = True Then
            ButtonBuyItemsBuy()
        End If

        If Controls.Dismiss(True, True, True) = True Then
            If BuyItemsShowDescription = True Then
                BuyItemsShowDescription = False
            Else
                MenuState = MenuStates.BuyItemsCategory
            End If
        End If

        BuySellSparkleRotation += 0.005F

        If BuySellItemShrinking = True Then
            BuySellItemSize -= 0.5F
            If BuySellItemSize <= 160.0F Then
                BuySellItemShrinking = False
            End If
        Else
            BuySellItemSize += 0.5F
            If BuySellItemSize >= 192.0F Then
                BuySellItemShrinking = True
            End If
        End If
    End Sub

    Private Sub ButtonBuyItemsMinus()
        If Controls.ShiftDown() = True Then
            BuyItemsAmount -= 5
        Else
            BuyItemsAmount -= 1
        End If
    End Sub

    Private Sub ButtonBuyItemsPlus()
        If Controls.ShiftDown() = True Then
            BuyItemsAmount += 5
        Else
            BuyItemsAmount += 1
        End If
    End Sub

    Private Sub ButtonBuyItemsBuy()
        If BuyItemsAmount > 0 Then
            Dim tradeItem As TradeItem = BuyItemsList(Scroll + Cursor)

            ChangeCurrencyAmount(-(tradeItem.Price * BuyItemsAmount))
            Core.Player.Inventory.AddItem(tradeItem.ItemID, BuyItemsAmount)

            'add a Premier Ball (ID=3) if the player bought 10 or more Poké Balls (ID=5):
            If tradeItem.ItemID = 5 And BuyItemsAmount >= 10 Then
                Core.Player.Inventory.AddItem(3, 1)
            End If

            'Remove trade item from seller's side if the rest amount is smaller than 0:
            If tradeItem.Amount > -1 Then
                For i = 0 To TradeItems.Count - 1
                    If TradeItems(i).ItemID = tradeItem.ItemID And tradeItem.Amount = TradeItems(i).Amount Then
                        Dim t As TradeItem = TradeItems(i)
                        t.Amount -= BuyItemsAmount

                        If t.Amount < 1 Then
                            TradeItems.RemoveAt(i)
                        Else
                            TradeItems(i) = t
                        End If
                        Exit For
                    End If
                Next
            End If

            LoadBuyItemsList()

            SoundManager.PlaySound("buy2")

            If BuyItemsList.Count = 0 Then
                MenuState = MenuStates.BuyItemsCategory
                LoadBuyCategoriesItems()
            End If
        End If
    End Sub

    Private Function GetMaxBuyItemAmount(ByVal tradeItem As TradeItem) As Integer
        Dim item As Item = tradeItem.GetItem()
        Dim maxAmount As Integer = item.MaxStack - Core.Player.Inventory.GetItemAmount(item.ID)

        If maxAmount > tradeItem.Amount And tradeItem.Amount > -1 Then
            maxAmount = tradeItem.Amount
        End If

        If tradeItem.Price = 0 Then
            Return maxAmount
        End If

        Dim money As Integer = GetCurrencyAmount()
        Dim amount As Integer = CInt(Math.Floor(money / tradeItem.Price))
        Return amount.Clamp(0, maxAmount)
    End Function

    Private Sub DrawBuyItems()
        'Item selection menu:
        For i = Scroll To Scroll + 5
            If i <= BuyItemsList.Count - 1 Then
                Dim p As Integer = i - Scroll

                DrawButton(New Vector2(100, 100 + p * 96), 5, BuyItemsList(i).GetItem().Name, 16, BuyItemsList(i).GetItem().Texture)
            End If
        Next

        If BuyItemsList.Count > 0 Then
            Dim selectedItem As TradeItem = BuyItemsList(Scroll + Cursor)

            'Item Preview:
            SpriteBatch.EndBatch()
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise)
            SpriteBatch.Draw(TextureManager.GetTexture("GUI\Box\Sparkle"), New Rectangle(736 + 128, 160 + 128, 256, 256), Nothing, Color.White, BuySellSparkleRotation, New Vector2(128, 128), SpriteEffects.None, 0.0F)
            SpriteBatch.End()
            SpriteBatch.BeginBatch()

            Dim itemOffset As Single = (256 - BuySellItemSize) / 2.0F
            SpriteBatch.Draw(selectedItem.GetItem().Texture, New Rectangle(CInt(736 + itemOffset), CInt(160 + itemOffset), CInt(BuySellItemSize), CInt(BuySellItemSize)), Color.White)

            If BuyItemsShowDescription = True Then
                Canvas.DrawRectangle(New Rectangle(736 + 28, 160 + 28, 200, 200), New Color(0, 0, 0, 200))
                Dim t As String = selectedItem.GetItem().Description.Crop(FontManager.MiniFont, 180)
                SpriteBatch.DrawString(FontManager.MiniFont, t, New Vector2(736 + 30, 160 + 30), Color.White)
            End If

            'Amount of item in bag:
            Dim amount As String = Core.Player.Inventory.GetItemAmount(selectedItem.ItemID).ToString()
            While amount.Length < 3
                amount = "0" & amount
            End While
            Dim bannerText As String = ""
            If selectedItem.Amount > -1 Then
                bannerText = " | In Stock: " & selectedItem.Amount
            End If
            DrawBanner(New Vector2(665, 430), 30, "In Inventory: " & amount & bannerText, FontManager.MiniFont, 400)

            '- button:
            SpriteBatch.Draw(texture, New Rectangle(664, 484, 64, 64), New Rectangle(16, 32, 16, 16), Color.White)
            SpriteBatch.DrawString(FontManager.MainFont, "-", New Vector2(664 + 23, 484 + 2), Color.Black, 0.0F, Vector2.Zero, 2.0F, SpriteEffects.None, 0.0F)

            'amount field:
            Canvas.DrawRectangle(New Rectangle(740, 492, 104, 48), New Color(77, 147, 198))
            Canvas.DrawRectangle(New Rectangle(744, 496, 96, 40), New Color(232, 240, 248))
            Dim amountString As String = BuyItemsAmount.ToString()
            While amountString.Length < 3
                amountString = "0" & amountString
            End While
            amountString = "x" & amountString
            SpriteBatch.DrawString(FontManager.MainFont, amountString, New Vector2(792 - FontManager.MainFont.MeasureString(amountString).X / 2.0F, 504), Color.Black)

            '+ button:
            SpriteBatch.Draw(texture, New Rectangle(856, 484, 64, 64), New Rectangle(16, 32, 16, 16), Color.White)
            SpriteBatch.DrawString(FontManager.MainFont, "+", New Vector2(856 + 19, 484 + 6), Color.Black, 0.0F, Vector2.Zero, 2.0F, SpriteEffects.None, 0.0F)

            SpriteBatch.DrawString(FontManager.MainFont, "Per Item: " & selectedItem.Price.ToString() & GetCurrencyShort() & vbNewLine &
                                                       "Total: " & (BuyItemsAmount * selectedItem.Price).ToString() & GetCurrencyShort(), New Vector2(930, 490), Color.White)

            'Buy button:
            If BuyItemsAmount > 0 Then
                If ControllerHandler.IsConnected() = True Then
                    SpriteBatch.Draw(TextureManager.GetTexture("GUI\GamePad\xboxControllerButtonA"), New Rectangle(664 + 12, 484 + 64 + 34, 40, 40), Color.White)
                End If

                DrawButton(New Vector2(664 + 64, 484 + 64 + 22), 1, "Buy", 64)
            End If
        End If

        'Current balance:
        DrawBanner(New Vector2(665, 110), 30, "Current balance: " & GetCurrencyDisplay(), FontManager.MiniFont, 400)

        'Cursor draw:
        DrawMainCursor()
    End Sub

#End Region

#Region "SellCatetoryScreen"

    Private loadedSellCategories As New List(Of Item.ItemTypes)

    Private Sub LoadSellCategoryItems()
        loadedSellCategories.Clear()

        For Each c In Core.Player.Inventory
            Dim i = Item.GetItemByID(c.ItemID)
            If loadedSellCategories.Contains(i.ItemType) = False And i.CanBeTraded = True Then
                loadedSellCategories.Add(i.ItemType)
            End If
        Next
        loadedSellCategories = (From c As Item.ItemTypes In loadedSellCategories Order By CInt(c)).ToList()
    End Sub

    Private Sub UpdateSellCategory()
        Title = "Sell Items"

        If loadedSellCategories.Count > 0 Then
            If Controls.Down(True, True, True, True, True, True) = True Then
                Cursor += 1
                If Controls.ShiftDown() = True Then
                    Cursor += 4
                End If
            End If
            If Controls.Up(True, True, True, True, True, True) = True Then
                Cursor -= 1
                If Controls.ShiftDown() = True Then
                    Cursor -= 4
                End If
            End If

            While Cursor > 5
                Cursor -= 1
                Scroll += 1
            End While
            While Cursor < 0
                Cursor += 1
                Scroll -= 1
            End While

            If loadedSellCategories.Count < 7 Then
                Scroll = 0
            Else
                Scroll = Scroll.Clamp(0, loadedSellCategories.Count - 6)
            End If
            If loadedSellCategories.Count < 6 Then
                Cursor = Cursor.Clamp(0, loadedSellCategories.Count - 1)
            Else
                Cursor = Cursor.Clamp(0, 5)
            End If

            If Controls.Accept(True, False, False) = True Then
                For i = Scroll To Scroll + 5
                    If i <= loadedSellCategories.Count - 1 Then
                        If New Rectangle(100, 100 + (i - Scroll) * 96, 64 * 7, 64).Contains(MouseHandler.MousePosition) = True Then
                            If i = Scroll + Cursor Then
                                ButtonSellCategoriesAccept()
                            Else
                                Cursor = i - Scroll
                            End If
                        End If
                    End If
                Next
            End If

            If Controls.Accept(False, True, True) = True Then
                ButtonSellCategoriesAccept()
            End If
        End If

        If Controls.Dismiss(True, True, True) = True Then
            MenuState = MenuStates.MainPage
        End If
    End Sub

    Private Sub ButtonSellCategoriesAccept()
        CurrentCategory = loadedSellCategories(Cursor + Scroll)
        MenuState = MenuStates.SellItems
        Cursor = 0
        Scroll = 0
        LoadSellItemsList()
    End Sub

    Private Sub DrawSellCategory()
        If loadedSellCategories.Count > 0 Then
            For i = Scroll To Scroll + 5
                If i <= loadedSellCategories.Count - 1 Then
                    Dim p As Integer = i - Scroll

                    DrawButton(New Vector2(100, 100 + p * 96), 5, Item.GetItemTypeName(loadedSellCategories(i)), 16, GetItemTypeTexture(loadedSellCategories(i)))
                End If
            Next

            DrawMainCursor()
        Else
            DrawBanner(New Vector2(CSng(windowSize.Width / 2 - 250), CSng(windowSize.Height / 2 - 50)), 100, "You have no items to sell.", FontManager.MainFont, 500)
        End If
    End Sub

#End Region

#Region "SellItemsScreen"

    Private SellItemsList As New List(Of TradeItem)
    Private SellItemsAmount As Integer = 1
    Private SellItemsShowDescription As Boolean = False

    Private Sub LoadSellItemsList()
        SellItemsList.Clear()
        For Each c In Core.Player.Inventory
            Dim i = Item.GetItemByID(c.ItemID)
            If i.CanBeTraded = True Then
                If i.ItemType = CurrentCategory Then
                    SellItemsList.Add(New TradeItem(i.ID, c.Amount, -1, Currency))
                End If
            End If
        Next
        SellItemsList = (From i As TradeItem In SellItemsList Order By i.GetItem().Name).ToList()
    End Sub

    Private Sub UpdateSellItems()
        Title = "Sell " & Item.GetItemTypeName(CurrentCategory)

        If Controls.Down(True, True, True, True, True, True) = True Then
            Cursor += 1
            If Controls.ShiftDown() = True Then
                Cursor += 4
            End If
        End If
        If Controls.Up(True, True, True, True, True, True) = True Then
            Cursor -= 1
            If Controls.ShiftDown() = True Then
                Cursor -= 4
            End If
        End If
        If Controls.Right(True, True, False, True, True, True) = True Then
            SellItemsAmount += 1
            If Controls.ShiftDown() = True Then
                SellItemsAmount += 4
            End If
        End If
        If Controls.Left(True, True, False, True, True, True) = True Then
            SellItemsAmount -= 1
            If Controls.ShiftDown() = True Then
                SellItemsAmount -= 4
            End If
        End If

        SellItemsClampCursor()

        If Controls.Accept(True, False, False) = True Then
            For i = Scroll To Scroll + 5
                If i <= SellItemsList.Count - 1 Then
                    If New Rectangle(100, 100 + (i - Scroll) * 96, 64 * 7, 64).Contains(MouseHandler.MousePosition) = True Then
                        Cursor = i - Scroll
                    End If
                End If
            Next

            'Item Description:
            If New Rectangle(736, 160, 256, 256).Contains(MouseHandler.MousePosition) = True Then
                SellItemsShowDescription = Not SellItemsShowDescription
            End If

            '- button:
            If New Rectangle(664, 484, 64, 64).Contains(MouseHandler.MousePosition) = True Then
                ButtonSellItemsMinus()
            End If
            '+ button:
            If New Rectangle(856, 484, 64, 64).Contains(MouseHandler.MousePosition) = True Then
                ButtonSellItemsPlus()
            End If

            'Buy button:
            If New Rectangle(664 + 64, 484 + 64 + 22, 64 * 3, 64).Contains(MouseHandler.MousePosition) = True Then
                ButtonSellItemsSell()
            End If
        End If

        If ControllerHandler.ButtonPressed(Buttons.Y) = True Or KeyBoardHandler.KeyPressed(KeyBindings.SpecialKey) = True Then
            SellItemsShowDescription = Not SellItemsShowDescription
        End If

        If SellItemsList.Count > 0 Then
            SellItemsAmount = SellItemsAmount.Clamp(0, SellItemsList(Scroll + Cursor).Amount)
        End If

        If Controls.Accept(False, True, True) = True Then
            ButtonSellItemsSell()
        End If

        If Controls.Dismiss(True, True, True) = True Then
            If SellItemsShowDescription = True Then
                SellItemsShowDescription = False
            Else
                MenuState = MenuStates.SellItemsCategory
            End If
        End If

        BuySellSparkleRotation += 0.005F

        If BuySellItemShrinking = True Then
            BuySellItemSize -= 0.5F
            If BuySellItemSize <= 160.0F Then
                BuySellItemShrinking = False
            End If
        Else
            BuySellItemSize += 0.5F
            If BuySellItemSize >= 192.0F Then
                BuySellItemShrinking = True
            End If
        End If
    End Sub

    Private Sub SellItemsClampCursor()
        While Cursor > 5
            Cursor -= 1
            Scroll += 1
        End While
        While Cursor < 0
            Cursor += 1
            Scroll -= 1
        End While

        If SellItemsList.Count < 7 Then
            Scroll = 0
        Else
            Scroll = Scroll.Clamp(0, SellItemsList.Count - 6)
        End If
        If SellItemsList.Count < 6 Then
            Cursor = Cursor.Clamp(0, SellItemsList.Count - 1)
        Else
            Cursor = Cursor.Clamp(0, 5)
        End If
    End Sub

    Private Sub ButtonSellItemsMinus()
        If Controls.ShiftDown() = True Then
            SellItemsAmount -= 5
        Else
            SellItemsAmount -= 1
        End If
    End Sub

    Private Sub ButtonSellItemsPlus()
        If Controls.ShiftDown() = True Then
            SellItemsAmount += 5
        Else
            SellItemsAmount += 1
        End If
    End Sub

    Private Sub ButtonSellItemsSell()
        If SellItemsAmount > 0 Then
            sellItemsConfirmationCursor = 0
            MenuState = MenuStates.SellItemsConfirmation
        End If
    End Sub

    Private Sub DrawSellItems()
        'Item selection menu:
        For i = Scroll To Scroll + 5
            If i <= SellItemsList.Count - 1 Then
                Dim p As Integer = i - Scroll

                DrawButton(New Vector2(100, 100 + p * 96), 5, SellItemsList(i).GetItem().Name, 16, SellItemsList(i).GetItem().Texture)
            End If
        Next

        If SellItemsList.Count > 0 Then
            Dim selectedItem As TradeItem = SellItemsList(Scroll + Cursor)

            'Item Preview:
            SpriteBatch.EndBatch()
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise)
            SpriteBatch.Draw(TextureManager.GetTexture("GUI\Box\Sparkle"), New Rectangle(736 + 128, 160 + 128, 256, 256), Nothing, Color.White, BuySellSparkleRotation, New Vector2(128, 128), SpriteEffects.None, 0.0F)
            SpriteBatch.End()
            SpriteBatch.BeginBatch()

            Dim itemOffset As Single = (256 - BuySellItemSize) / 2.0F
            SpriteBatch.Draw(selectedItem.GetItem().Texture, New Rectangle(CInt(736 + itemOffset), CInt(160 + itemOffset), CInt(BuySellItemSize), CInt(BuySellItemSize)), Color.White)

            If SellItemsShowDescription = True Then
                Canvas.DrawRectangle(New Rectangle(736 + 28, 160 + 28, 200, 200), New Color(0, 0, 0, 200))
                Dim t As String = selectedItem.GetItem().Description.Crop(FontManager.MiniFont, 180)
                SpriteBatch.DrawString(FontManager.MiniFont, t, New Vector2(736 + 30, 160 + 30), Color.White)
            End If

            'Amount of item in bag:
            Dim amount As String = Core.Player.Inventory.GetItemAmount(selectedItem.ItemID).ToString()
            While amount.Length < 3
                amount = "0" & amount
            End While
            DrawBanner(New Vector2(665, 430), 30, "In Inventory: " & amount, FontManager.MiniFont, 400)

            '- button:
            SpriteBatch.Draw(texture, New Rectangle(664, 484, 64, 64), New Rectangle(16, 32, 16, 16), Color.White)
            SpriteBatch.DrawString(FontManager.MainFont, "-", New Vector2(664 + 23, 484 + 2), Color.Black, 0.0F, Vector2.Zero, 2.0F, SpriteEffects.None, 0.0F)

            'amount field:
            Canvas.DrawRectangle(New Rectangle(740, 492, 104, 48), New Color(77, 147, 198))
            Canvas.DrawRectangle(New Rectangle(744, 496, 96, 40), New Color(232, 240, 248))
            Dim amountString As String = SellItemsAmount.ToString()
            While amountString.Length < 3
                amountString = "0" & amountString
            End While
            amountString = "x" & amountString
            SpriteBatch.DrawString(FontManager.MainFont, amountString, New Vector2(792 - FontManager.MainFont.MeasureString(amountString).X / 2.0F, 504), Color.Black)

            '+ button:
            SpriteBatch.Draw(texture, New Rectangle(856, 484, 64, 64), New Rectangle(16, 32, 16, 16), Color.White)
            SpriteBatch.DrawString(FontManager.MainFont, "+", New Vector2(856 + 19, 484 + 6), Color.Black, 0.0F, Vector2.Zero, 2.0F, SpriteEffects.None, 0.0F)

            SpriteBatch.DrawString(FontManager.MainFont, "Per Item: " & selectedItem.SellPrice().ToString() & GetCurrencyShort() & vbNewLine &
                                                       "Total: " & (SellItemsAmount * selectedItem.SellPrice()).ToString() & GetCurrencyShort(), New Vector2(930, 490), Color.White)

            'Sell button:
            If SellItemsAmount > 0 Then
                If ControllerHandler.IsConnected() = True Then
                    SpriteBatch.Draw(TextureManager.GetTexture("GUI\GamePad\xboxControllerButtonA"), New Rectangle(664 + 12, 484 + 64 + 34, 40, 40), Color.White)
                End If

                DrawButton(New Vector2(664 + 64, 484 + 64 + 22), 1, "Sell", 64)
            End If
        End If

        'Current balance:
        DrawBanner(New Vector2(665, 110), 30, "Current balance: " & GetCurrencyDisplay(), FontManager.MiniFont, 400)

        'Cursor draw:
        DrawMainCursor()
    End Sub

#End Region

#Region "SellItemsConfirmationScreen"

    Private sellItemsConfirmationCursor As Integer = 0

    Private Sub UpdateSellConfirmation()
        If Controls.Down(True, True, True, True, True, True) = True Then
            sellItemsConfirmationCursor += 1
        End If
        If Controls.Up(True, True, True, True, True, True) = True Then
            sellItemsConfirmationCursor -= 1
        End If

        sellItemsConfirmationCursor = sellItemsConfirmationCursor.Clamp(0, 1)

        If Controls.Accept(True, False, False) = True Then
            For i = 0 To 1
                If New Rectangle(CInt(windowSize.Width / 2.0F - 192), CInt(windowSize.Height / 2.0F - 60 + (i * 96)), 64 * 5, 64).Contains(MouseHandler.MousePosition) = True Then
                    If sellItemsConfirmationCursor = i Then
                        Select Case i
                            Case 0
                                ButtonSellConfirmationSell()
                            Case 1
                                ButtonSellConfirmationCancel()
                        End Select
                    Else
                        sellItemsConfirmationCursor = i
                    End If
                End If
            Next
        End If

        If Controls.Accept(False, True, True) = True Then
            Select Case sellItemsConfirmationCursor
                Case 0
                    ButtonSellConfirmationSell()
                Case 1
                    ButtonSellConfirmationCancel()
            End Select
        End If
    End Sub

    Private Sub DrawSellConfirmation()
        DrawSellItems()

        Canvas.DrawRectangle(New Rectangle(CInt(windowSize.Width / 2 - 300), CInt(windowSize.Height / 2 - 200), 600, 400), New Color(0, 0, 0, 150))

        Dim tradeItem As TradeItem = SellItemsList(Scroll + Cursor)

        Dim text As String = "Do you want to sell" & vbNewLine & SellItemsAmount & " " & tradeItem.GetItem().Name & "?"

        SpriteBatch.DrawString(FontManager.MiniFont,
                                    text,
                                    New Vector2(windowSize.Width / 2.0F - FontManager.MiniFont.MeasureString(text).X, windowSize.Height / 2.0F - 170),
                                    Color.White, 0.0F, Vector2.Zero, 2.0F, SpriteEffects.None, 0.0F)

        DrawButton(New Vector2(windowSize.Width / 2.0F - 192, windowSize.Height / 2.0F - 60), 4, "Sell", 16, Nothing)
        DrawButton(New Vector2(windowSize.Width / 2.0F - 192, windowSize.Height / 2.0F + 36), 4, "Cancel", 16, Nothing)

        'Cursor:
        Dim cPosition As Vector2 = New Vector2(windowSize.Width / 2.0F - 192 + 280, windowSize.Height / 2.0F - 60 + sellItemsConfirmationCursor * 96 - 42)

        Dim t As Texture2D = TextureManager.GetTexture("GUI\Menus\General", New Rectangle(0, 0, 16, 16), "")
        SpriteBatch.Draw(t, New Rectangle(CInt(cPosition.X), CInt(cPosition.Y), 64, 64), Color.White)
    End Sub

    Private Sub ButtonSellConfirmationSell()
        Dim tradeItem As TradeItem = SellItemsList(Scroll + Cursor)

        ChangeCurrencyAmount(tradeItem.SellPrice() * SellItemsAmount)
        Core.Player.Inventory.RemoveItem(tradeItem.ItemID, SellItemsAmount)
        LoadSellItemsList()
        SellItemsClampCursor()
        SoundManager.PlaySound("buy2")

        If SellItemsList.Count = 0 Then
            MenuState = MenuStates.SellItemsCategory
            LoadSellCategoryItems()
        Else
            MenuState = MenuStates.SellItems
        End If
    End Sub

    Private Sub ButtonSellConfirmationCancel()
        MenuState = MenuStates.SellItems
    End Sub

#End Region

    ''' <summary>
    ''' Draws a button on the screen.
    ''' </summary>
    ''' <param name="Position">The position of the button in absolute screen coordinates.</param>
    ''' <param name="Width">The width of the button in 64px steps. 5 is default.</param>
    ''' <param name="Text">The text to be displayed on the button.</param>
    ''' <param name="TextOffset">The offset on the text on the button. 16 is default.</param>
        Private Sub DrawButton(ByVal Position As Vector2, ByVal Width As Integer, ByVal Text As String, ByVal TextOffset As Integer, Optional ByVal Image As Texture2D = Nothing)
        SpriteBatch.Draw(texture, New Rectangle(CInt(Position.X), CInt(Position.Y), 64, 64), New Rectangle(16, 16, 16, 16), Color.White)
        SpriteBatch.Draw(texture, New Rectangle(CInt(Position.X) + 64, CInt(Position.Y), 64 * Width, 64), New Rectangle(32, 16, 16, 16), Color.White)
        SpriteBatch.Draw(texture, New Rectangle(CInt(Position.X) + 64 * (Width + 1), CInt(Position.Y), 64, 64), New Rectangle(16, 16, 16, 16), Color.White, 0.0F, Vector2.Zero, SpriteEffects.FlipHorizontally, 0.0F)

        If Image Is Nothing Then
            SpriteBatch.DrawString(FontManager.MainFont, Text, New Vector2(TextOffset + CInt(Position.X), CInt(Position.Y) + 16), Color.Black, 0.0F, Vector2.Zero, 1.25F, SpriteEffects.None, 0.0F)
        Else
            SpriteBatch.DrawString(FontManager.MainFont, Text, New Vector2(4 + 16 + Image.Width + TextOffset + CInt(Position.X), CInt(Position.Y) + 16), Color.Black, 0.0F, Vector2.Zero, 1.25F, SpriteEffects.None, 0.0F)
            SpriteBatch.Draw(Image, New Rectangle(CInt(Position.X) + TextOffset + 8, CInt(Position.Y) + 19, Image.Width, Image.Height), Color.White)
        End If
    End Sub

    Private Sub DrawBanner(ByVal Position As Vector2, ByVal Height As Integer, ByVal Text As String, ByVal Font As SpriteFont, Optional ByVal FixedWidth As Integer? = Nothing)
        Dim textWidth As Single = Font.MeasureString(Text).X
        Dim textHeight As Single = Font.MeasureString(Text).Y
        Dim textY As Single = (Height / 2.0F) - (textHeight / 2.0F)
        Dim Width As Integer = CInt((Height + 10) * 2 + textWidth)

        If FixedWidth.HasValue = True Then
            Width = FixedWidth.GetValueOrDefault()
        End If

        Canvas.DrawGradient(New Rectangle(CInt(Position.X), CInt(Position.Y), Height, Height), New Color(42, 167, 198, 0), New Color(42, 167, 198, 150), True, -1)
        Canvas.DrawRectangle(New Rectangle(CInt(Position.X) + Height, CInt(Position.Y), Width - (Height * 2), Height), New Color(42, 167, 198, 150))
        Canvas.DrawGradient(New Rectangle(CInt(Position.X) + (Width - Height), CInt(Position.Y), Height, Height), New Color(42, 167, 198, 150), New Color(42, 167, 198, 0), True, -1)

        SpriteBatch.DrawString(Font, Text, New Vector2(Position.X + Height + 10, Position.Y + textY), Color.White)
    End Sub

    Private Function GetItemTypeTexture(ByVal itemType As Item.ItemTypes) As Texture2D
        Dim i As Integer = 0
        Select Case itemType
            Case Item.ItemTypes.Standard
                i = 0
            Case Item.ItemTypes.Medicine
                i = 1
            Case Item.ItemTypes.Machines
                i = 2
            Case Item.ItemTypes.Pokéballs
                i = 3
            Case Item.ItemTypes.Plants
                i = 4
            Case Item.ItemTypes.Mail
                i = 5
            Case Item.ItemTypes.BattleItems
                i = 6
            Case Item.ItemTypes.KeyItems
                i = 7
        End Select

        Return TextureManager.TextureRectangle(TextureManager.GetTexture("GUI\Menus\BagPack"), New Rectangle(i * 24, 150, 24, 24))
    End Function

    Private Function GetCurrencyAmount() As Integer
        Select Case Currency
            Case Currencies.BattlePoints
                Return Core.Player.BP
            Case Currencies.Pokédollar
                Return Core.Player.Money
        End Select
        Return 0
    End Function

    Private Function GetCurrencyDisplay() As String
        Select Case Currency
            Case Currencies.BattlePoints
                Return GetCurrencyAmount().ToString() & " Battle Points"
            Case Currencies.Pokédollar
                Return GetCurrencyAmount().ToString() & " Pokédollar"
        End Select
        Return ""
    End Function

    Private Function GetCurrencyShort() As String
        Select Case Currency
            Case Currencies.BattlePoints
                Return "BP"
            Case Currencies.Pokédollar
                Return "$"
        End Select
        Return ""
    End Function

    Private Sub ChangeCurrencyAmount(ByVal change As Integer)
        Select Case Currency
            Case Currencies.BattlePoints
                Core.Player.BP = (Core.Player.BP + change).Clamp(0, Integer.MaxValue)
            Case Currencies.Pokédollar
                Core.Player.Money = (Core.Player.Money + change).Clamp(0, Integer.MaxValue)
        End Select
    End Sub

End Class