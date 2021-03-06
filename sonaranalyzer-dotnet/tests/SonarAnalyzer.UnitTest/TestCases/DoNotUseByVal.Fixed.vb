Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class Foo
        Public Function NoByVal(parameter As String) As String ' Compliant
            Return byValParameter
        End Function

        Public Function OptionalNoByVal(Optional parameter As String = "Default") As String ' Compliant
            Return byValParameter
        End Function

        Public Function WithByVal(byValParameter As String) As String ' Fixed
            Return byValParameter
        End Function

        Public Function SecondParameter(parameter As String, byValParameter As String) As String ' Fixed
            Return byValParameter
        End Function

        Public Function OptionalWithByVal(Optional byValParameter As String = "Default") As String ' Fixed
            Return byValParameter
        End Function

        Public Function NoByVal(ByRef parameter As String) As String ' Compliant
            Return byValParameter
        End Function

        Public Function OptionalNoByVal(Optional ByRef parameter As String = "Default") As String ' Compliant
            Return byValParameter
        End Function
    End Class
End Namespace
