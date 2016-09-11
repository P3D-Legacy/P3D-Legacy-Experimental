﻿Namespace Items.KeyItems

    Public Class GoodRod

        Inherits Item

        Public Sub New()
            MyBase.New("Good Rod", 1337, ItemTypes.KeyItems, 59, 1, 0, New Rectangle(264, 48, 24, 24), "A new, good-quality fishing rod. Use it by any body of water to fish for wild aquatic Pokémon.")

            Me._canBeUsed = True
            Me._canBeUsedInBattle = False
            Me._canBeTraded = False
            Me._canBeHold = False
            Me._canBeTossed = False
        End Sub

        Public Overrides Sub Use()
            If OldRod.IsInfrontOfWater() = True And Screen.Level.Surfing = False And Screen.Level.Riding = False Then
                Dim s As String = ""

                While Core.CurrentScreen.Identification <> Screen.Identifications.OverworldScreen
                    Core.CurrentScreen = Core.CurrentScreen.PreScreen
                End While

                Dim p As Pokemon = Nothing

                Dim pokeFile As String = Screen.Level.LevelFile.Remove(Screen.Level.LevelFile.Length - 4, 4) & ".poke"
                If System.IO.File.Exists(GameModeManager.GetPokeFilePath(pokeFile)) = True Then
                    p = Spawner.GetPokemon(Screen.Level.LevelFile, Spawner.EncounterMethods.GoodRod, False)
                End If

                If p Is Nothing Then
                    p = Pokemon.GetPokemonByID(129)
                    p.Generate(20, True)
                End If

                Dim PokemonID As Integer = p.Number
                Dim PokemonShiny As String = "N"
                If p.IsShiny = True Then
                    PokemonShiny = "S"
                End If

                If Core.Random.Next(0, 3) <> 0 Or
                    Core.Player.Pokemons(0).Ability.Name.ToLower() = "suction cups" Or
                    Core.Player.Pokemons(0).Ability.Name.ToLower() = "sticky hold" Then

                    Dim LookingOffset As New Vector3(0)

                    Select Case Screen.Camera.GetPlayerFacingDirection()
                        Case 0
                            LookingOffset.Z = -1
                        Case 1
                            LookingOffset.X = -1
                        Case 2
                            LookingOffset.Z = 1
                        Case 3
                            LookingOffset.X = 1
                    End Select

                    Dim spawnPosition As Vector3 = New Vector3(Screen.Camera.Position.X + LookingOffset.X, Screen.Camera.Position.Y, Screen.Camera.Position.Z + LookingOffset.Z)

                    Dim endRotation As Integer = Screen.Camera.GetPlayerFacingDirection() + 2
                    If endRotation > 3 Then
                        endRotation = endRotation - 4
                    End If

                    s &= "@player.showrod(1)" & vbNewLine &
                        "@text.show(. . . . . . . . . .)" & vbNewLine &
                        "@text.show(Oh!~A bite!)" & vbNewLine &
                        "@player.hiderod" & vbNewLine &
                        "@npc.spawn(" & spawnPosition.X.ToString().Replace(GameController.DecSeparator, ".") & "," & spawnPosition.Y.ToString().Replace(GameController.DecSeparator, ".") & "," & spawnPosition.Z.ToString().Replace(GameController.DecSeparator, ".") & ",0,...,[POKEMON|" & PokemonShiny & "]" & PokemonID & PokemonForms.GetOverworldAddition(p) & ",0," & endRotation & ",POKEMON,1337,Still)" & vbNewLine &
                        "@Level.Update" & vbNewLine &
                        "@pokemon.cry(" & PokemonID & ")" & vbNewLine &
                        "@level.wait(50)" & vbNewLine &
                        "@text.show(The wild " & p.OriginalName & "~attacked!)" & vbNewLine &
                        "@npc.remove(1337)" & vbNewLine &
                        "@battle.setvar(divebattle,true)" & vbNewLine &
                        "@battle.wild(" & p.GetSaveData() & ")"
                Else
                    s &= "@player.showrod(1)" & vbNewLine &
                        "@text.show(. . . . . . . . . .)" & vbNewLine &
                        "@text.show(No, there's nothing here...)" & vbNewLine &
                        "@player.hiderod"
                End If

                Construct.Controller.GetInstance().RunFromString(s, {Construct.Controller.ScriptRunOptions.CheckDelay})
            Else
                Screen.TextBox.Show("Now is not the time~to use that.", {}, True, True)
            End If
        End Sub

    End Class

End Namespace