﻿Namespace Items.Medicine

    Public Class MaxEther

        Inherits Item

        Public Sub New()
            MyBase.New("Max Ether", 2000, ItemTypes.Medicine, 64, 1, 1, New Rectangle(384, 48, 24, 24), "This medicine can fully restore the PP of a single selected move that has been learned by a Pokémon.")

            Me._canBeUsed = True
            Me._canBeUsedInBattle = True
            Me._canBeTraded = True
            Me._canBeHold = True
        End Sub

        Public Overrides Sub Use()
            Core.SetScreen(New ChoosePokemonScreen(Core.CurrentScreen, Me, AddressOf Me.UseOnPokemon, "Use " & Me.Name, True))
        End Sub

        Public Overrides Function UseOnPokemon(ByVal PokeIndex As Integer) As Boolean
            Core.SetScreen(New ChooseAttackScreen(Core.CurrentScreen, Core.Player.Pokemons(PokeIndex), True, True, AddressOf UseOnAttack))
            Return True
        End Function

        Private Sub UseOnAttack(ByVal Pokemon As Pokemon, ByVal AttackIndex As Integer)
            If Pokemon.Attacks(AttackIndex).CurrentPP < Pokemon.Attacks(AttackIndex).MaxPP Then
                Pokemon.Attacks(AttackIndex).CurrentPP = Pokemon.Attacks(AttackIndex).MaxPP

                Dim t As String = "Restored PP of~" & Pokemon.Attacks(AttackIndex).Name & "."
                t &= RemoveItem()
                PlayerStatistics.Track("[17]Medicine Items used", 1)

                SoundManager.PlaySound("single_heal", False)
                Screen.TextBox.Show(t, {}, True, True)
            Else
                Screen.TextBox.Show("The move already has~full PP.", {}, True, True)
            End If
        End Sub

    End Class

End Namespace