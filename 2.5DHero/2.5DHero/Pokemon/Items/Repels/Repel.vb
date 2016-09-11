﻿Namespace Items.Repels

    Public Class Repel

        Inherits Item

        Public Sub New()
            MyBase.New("Repel", 350, ItemTypes.Standard, 20, 1, 0, New Rectangle(432, 0, 24, 24), "An item that prevents any low-level wild Pokémon from jumping out at you for 100 steps after its use.")

            Me._canBeHold = True
            Me._canBeTraded = True
            Me._canBeUsed = True
            Me._canBeUsedInBattle = False
        End Sub

        Public Overrides Sub Use()
            If Core.Player.RepelSteps <= 0 Then
                Core.Player.Inventory.RemoveItem(Me.ID, 1)
                Core.Player.Temp.LastUsedRepel = Me.ID

                SoundManager.PlaySound("repel_use", False)
                Screen.TextBox.Show(Core.Player.Name & " used a~" & Me.Name, {}, True, True)
                Core.Player.RepelSteps = 100
                PlayerStatistics.Track("[42]Repels used", 1)
            Else
                Screen.TextBox.Show("The Repel is still~in effect.", {}, True, True)
            End If
        End Sub

    End Class

End Namespace