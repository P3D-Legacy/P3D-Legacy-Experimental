﻿Namespace Items.Medicine

    Public Class MaxRevive

        Inherits Item

        Public Sub New()
            MyBase.New("Max Revive", 4000, ItemTypes.Medicine, 40, 1, 0, New Rectangle(384, 24, 24, 24), "A medicine that can revive fainted Pokémon. It also fully restores a fainted Pokémon's maximum HP.")

            Me._canBeUsed = True
            Me._canBeUsedInBattle = True
            Me._canBeTraded = True
            Me._canBeHold = True

            Me._isHealingItem = True
        End Sub

        Public Overrides Sub Use()
            If CBool(GameModeManager.GetGameRuleValue("CanUseHealItem", "1")) = False Then
                Screen.TextBox.Show("Cannot use heal items.", {}, False, False)
                Exit Sub
            End If
            Core.SetScreen(New ChoosePokemonScreen(Core.CurrentScreen, Me, AddressOf Me.UseOnPokemon, "Use " & Me.Name, True))
        End Sub

        Public Overrides Function UseOnPokemon(ByVal PokeIndex As Integer) As Boolean
            Dim Pokemon As Pokemon = Core.Player.Pokemons(PokeIndex)

            If Pokemon.Status = Pokemon3D.Pokemon.StatusProblems.Fainted Then
                Pokemon.Status = Pokemon3D.Pokemon.StatusProblems.None
                Pokemon.HP = Pokemon.MaxHP

                SoundManager.PlaySound("single_heal", False)
                Screen.TextBox.Show(Pokemon.GetDisplayName() & "~is revitalized.", {}, False, False)
                PlayerStatistics.Track("[17]Medicine Items used", 1)

                RemoveItem()

                Return True
            Else
                Screen.TextBox.Show("Cannot use Max Revive~on this Pokémon.", {}, False, False)

                Return False
            End If
        End Function

    End Class

End Namespace