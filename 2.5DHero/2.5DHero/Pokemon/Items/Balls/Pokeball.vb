﻿Namespace Items.Balls

    Public Class Pokeball

        Inherits Item

        Public Sub New()
            MyBase.New("Poké Ball", 200, ItemTypes.Pokéballs, 5, 1, 0, New Rectangle(96, 0, 24, 24), "An item for catching Pokémon.")

            Me._isBall = True
            Me._canBeUsed = False
        End Sub

    End Class

End Namespace