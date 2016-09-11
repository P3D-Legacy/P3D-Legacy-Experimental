﻿Public Module Core

    Public SpriteBatch As CoreSpriteBatch
    Public FontRenderer As SpriteBatch

    Public GraphicsDevice As GraphicsDevice
    Public GraphicsManager As GraphicsDeviceManager
    Public Content As ContentManager
    Public GameTime As GameTime
    Public GameInstance As GameController
    Public Random As System.Random = New System.Random()

    Public KeyboardInput As KeyboardInput

    Public window As GameWindow
    Public windowSize As Rectangle = New Rectangle(0, 0, 1200, 680)
    Public GameMessage As GameMessage

    Public ServersManager As Servers.ServersManager

    Public CurrentScreen As Screen

    Public Player As Player
    Public GameJoltSave As GameJolt.GamejoltSave

    Public GameOptions As GameOptions

    Public sampler As SamplerState

    Public BackgroundColor As Color = New Color(173, 216, 255)

    Public OffsetMaps As New Dictionary(Of String, List(Of List(Of Entity)))

    Public Sub Initialize(ByVal gameReference As GameController)
        GameInstance = gameReference

        GraphicsManager = GameInstance.Graphics
        GraphicsDevice = GameInstance.GraphicsDevice
        Content = GameInstance.Content
        Content.RootDirectory = ""
        SpriteBatch = New CoreSpriteBatch(GraphicsDevice)
        FontRenderer = New CoreSpriteBatch(GraphicsDevice)
        window = GameInstance.Window

        If GameCore.CommandLineArgHandler.ForceGraphics = True Then
            window.Title = GameController.GAMENAME & " " & GameController.GAMEDEVELOPMENTSTAGE & " (FORCED GRAPHICS)"
        Else
            window.Title = GameController.GAMENAME & " " & GameController.GAMEDEVELOPMENTSTAGE
        End If

        GameOptions = New GameOptions()
        GameOptions.Load()

        GraphicsManager.PreferredBackBufferWidth = CInt(GameOptions.WindowSize.X)
        GraphicsManager.PreferredBackBufferHeight = CInt(GameOptions.WindowSize.Y)
        GraphicsDevice.PresentationParameters.BackBufferFormat = SurfaceFormat.Rgba1010102
        GraphicsDevice.PresentationParameters.DepthStencilFormat = DepthFormat.Depth24Stencil8

        windowSize = New Rectangle(0, 0, CInt(GameOptions.WindowSize.X), CInt(GameOptions.WindowSize.Y))

        GraphicsManager.PreferMultiSampling = True

        GraphicsManager.ApplyChanges()

        Canvas.SetupCanvas()
        Player = New Player()
        GameJoltSave = New GameJolt.GamejoltSave()

        GameMessage = New GameMessage(Nothing, New Size(0, 0), New Vector2(0, 0))

        sampler = New SamplerState()
        sampler.Filter = TextureFilter.Point
        sampler.AddressU = TextureAddressMode.Clamp
        sampler.AddressV = TextureAddressMode.Clamp

        ServersManager = New Servers.ServersManager()

        GraphicsDevice.SamplerStates(0) = sampler
        KeyboardInput = New KeyboardInput()
        SetScreen(New SplashScreen(GameInstance))
    End Sub

    Public Sub LoadContent()
        KeyBindings.Load()

        GameModeManager.LoadGameModes()
        Logger.Debug("162", "Loaded game modes.")

        FontManager.LoadFonts()

        Screen.TextBox.TextFont = FontManager.GetFontContainer("textfont")
        Logger.Debug("163", "Loaded fonts.")

        TextureManager.InitializeTextures()
        Logger.Debug("164", "Loaded content.")

        Logger.Debug("165", "Validated files. Result: " & Security.FileValidation.IsValid(True).ToString())
        If Security.FileValidation.IsValid(False) = False Then
            Logger.Log("250", Logger.LogTypes.Warning, "Core.vb: File Validation failed! Download a fresh copy of the game to fix this issue.")
        End If

        GameMessage = New GameMessage(Pokemon3D.TextureManager.DefaultTexture, New Size(10, 40), New Vector2(0, 0))
        GameMessage.Dock = Pokemon3D.GameMessage.DockStyles.Top
        GameMessage.BackgroundColor = Color.Black
        GameMessage.TextPosition = New Vector2(10, 10)
        Logger.Debug("166", "Gamemessage initialized.")

        GameOptions.Load()

        If System.IO.Directory.Exists(GameController.GamePath & "\Temp") = True Then
            Try
                System.IO.Directory.Delete(GameController.GamePath & "\Temp", True)
                Logger.Log("251", Logger.LogTypes.Message, "Core.vb: Deleted Temp directory.")
            Catch ex As Exception
                Logger.Log("252", Logger.LogTypes.Warning, "Core.vb: Failed to delete the Temp directory.")
            End Try
        End If

        GameJolt.StaffProfile.SetupStaff()
    End Sub

    Public Sub Update(ByVal gameTime As GameTime)
        If KeyBindings.IsInitialized Then
            Core.GameTime = gameTime

            KeyBoardHandler.Update()
            ControllerHandler.Update()

            ConnectScreen.UpdateConnectSet()

            If Core.GameInstance.IsActive = False Then
                If Core.CurrentScreen.CanBePaused = True Then
                    Core.SetScreen(New PauseScreen(Core.CurrentScreen))
                End If
            Else
                If KeyBoardHandler.KeyPressed(KeyBindings.EscapeKey) = True Or ControllerHandler.ButtonDown(Buttons.Start) = True Then
                    CurrentScreen.EscapePressed()
                End If
            End If

            CurrentScreen.Update()
            If CurrentScreen.CanChat = True Then
                If KeyBoardHandler.KeyPressed(KeyBindings.ChatKey) = True Then
                    OpenChatScreen()
                End If
            End If

            MainGameFunctions.FunctionKeys()

            MusicPlayer.GetInstance().Update(gameTime)

            GameMessage.Update()
            Controls.MakeMouseVisible()

            MouseHandler.Update()

            LoadingDots.Update()
            ForcedCrash.Update()

            ServersManager.Update()
        Else
            'Update the current screen when the keybindings have not been initialized, because this happens in the Update method of the SplashScreen.
            CurrentScreen.Update()
        End If
    End Sub

    Public Sub OpenChatScreen()
        If JoinServerScreen.Online = True Or Player.SandBoxMode = True Or GameController.IS_DEBUG_ACTIVE = True Then
            SetScreen(New ChatScreen(CurrentScreen))
        End If
    End Sub

    Public Sub Draw()
        GraphicsDevice.Clear(BackgroundColor)

        If SpriteBatch.Running = True Then
            SpriteBatch.EndBatch()
        Else
            SpriteBatch.BeginBatch()
            FontRenderer.Begin()

            GraphicsDevice.DepthStencilState = DepthStencilState.Default

            GraphicsDevice.SamplerStates(0) = sampler
            CurrentScreen.Draw()

            If Not Core.Player Is Nothing Then
                If Core.Player.IsGameJoltSave = True Then
                    GameJolt.Emblem.DrawNewEmblems()
                End If
                Core.Player.DrawLevelUp()
            End If

            If JoinServerScreen.Online = True Or Player.SandBoxMode = True Or GameController.IS_DEBUG_ACTIVE = True Then
                If CurrentScreen.Identification <> Screen.Identifications.ChatScreen Then
                    ChatScreen.DrawNewMessages()
                End If
            End If

            If GameOptions.ShowDebug > 0 Then
                DebugDisplay.Draw()
            End If

            GameMessage.Draw()
            OnlineStatus.Draw()

            Logger.DrawLog()

            SpriteBatch.EndBatch()
            FontRenderer.End()
        End If
    End Sub

    Public Sub Render()
        CurrentScreen.Render()
    End Sub

    Public Sub SetScreen(ByVal newScreen As Screen)
        If Not CurrentScreen Is Nothing Then
            CurrentScreen.ChangeFrom()
        End If

        CurrentScreen = newScreen

        If ControllerHandler.IsConnected() = True Then
            If GameInstance.IsMouseVisible = True And newScreen.MouseVisible = True Then
                GameInstance.IsMouseVisible = True
            Else
                GameInstance.IsMouseVisible = False
            End If
        Else
            GameInstance.IsMouseVisible = CurrentScreen.MouseVisible
        End If

        CurrentScreen.ChangeTo()
    End Sub

    Public Function GetMiddlePosition(ByVal OffsetFull As Size) As Vector2
        Dim v As New Vector2(CSng(Core.windowSize.Width / 2) - CSng(OffsetFull.Width / 2), CSng(Core.windowSize.Height / 2) - CSng(OffsetFull.Height / 2))

        Return v
    End Function

    Public Sub StartThreadedSub(ByVal s As System.Threading.ParameterizedThreadStart)
        Dim t As New Threading.Thread(s)
        t.IsBackground = True
        t.Start()
    End Sub

    Public ReadOnly Property ScreenSize() As Rectangle
        Get
            Dim x As Double = SpriteBatch.InterfaceScale()
            If x = 1D Then Return Core.windowSize
            Return New Rectangle(CInt(Core.windowSize.X / x), CInt(Core.windowSize.Y / x), CInt(Core.windowSize.Width / x), CInt(Core.windowSize.Height / x))
        End Get
    End Property

    Public ReadOnly Property ScaleScreenRec(ByVal rec As Rectangle) As Rectangle
        Get
            Dim x As Double = SpriteBatch.InterfaceScale()
            If x = 1D Then Return rec
            Return New Rectangle(CInt(rec.X * x), CInt(rec.Y * x), CInt(rec.Width * x), CInt(rec.Height * x))
        End Get
    End Property

    Public ReadOnly Property ScaleScreenVec(ByVal vec As Vector2) As Vector2
        Get
            Dim x As Double = SpriteBatch.InterfaceScale()
            If x = 1D Then Return vec
            Return New Vector2(CSng(vec.X * x), CSng(vec.Y * x))
        End Get
    End Property

    Public Sub SetWindowSize(ByVal Size As Vector2)
        GraphicsManager.PreferredBackBufferWidth = CInt(Size.X)
        GraphicsManager.PreferredBackBufferHeight = CInt(Size.Y)

        GraphicsManager.ApplyChanges()

        windowSize = New Rectangle(0, 0, CInt(Size.X), CInt(Size.Y))
    End Sub

End Module