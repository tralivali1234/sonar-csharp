﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class ExecutableLinesWalkerTest
    {
        private static void AssertLinesOfCode(string code, params int[] expectedExecutableLines)
        {
            var walker = new Metrics.CSharp.ExecutableLinesWalker();
            walker.Visit(CSharpSyntaxTree.ParseText(code).GetRoot());
            walker.ExecutableLines.Should().BeEquivalentTo(expectedExecutableLines);
        }

        [TestMethod]
        public void Test_01_No_Executable_Lines()
        {
            AssertLinesOfCode(
              @"using System;
                using System.Linq;

                namespace Test
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                        }
                    }
                }");
        }

        [TestMethod]
        public void Test_02_Checked_Unchecked()
        {
            AssertLinesOfCode(
              @"
                static void Main(string[] args)
                {
                    checked // +1
                    {
                        unchecked // +1
                        {
                        }
                    }
                }",
                4, 6);
        }

        [TestMethod]
        public void Test_03_Blocks()
        {
            AssertLinesOfCode(
              @"
                unsafe static void Main(int[] arr, object obj)
                {
                    lock (obj) { } // +1
                    fixed (int* p = arr) { } // +1
                    unsafe { } // +1
                    using ((IDisposable)obj) { } // +1
                }",
                4 ,5 ,6, 7);
        }

        [TestMethod]
        public void Test_04_Statements()
        {
            AssertLinesOfCode(
              @"
                void Foo(int i)
                {
                    ; // +1
                    i++; // +1
                }",
                4, 5);
        }

        [TestMethod]
        public void Test_06_Loops()
        {
            AssertLinesOfCode(
              @"
                void Foo(int[] arr)
                {
                    do {} // +1
                        while (true);
                    foreach (var a in arr) { }// +1
                    for (;;) { } // +1
                    while // +1
                        (true) { }
                }",
                4, 6, 7, 8);
        }

        [TestMethod]
        public void Test_07_Conditionals()
        {
            AssertLinesOfCode(
              @"
                void Foo(int? i, string s)
                {
                    if (true) { } // +1
                    label: // +1
                    switch (i) // +1
                    {
                        case 1:
                        case 2:
                        default:
                            break; // +1
                    }
                    var x = s?.Length; // +1
                    var xx = i == 1 ? 1 : 1; // +1
                }",
                4, 5, 6, 11, 13, 14);
        }

        [TestMethod]
        public void Test_08_Conditionals()
        {
            AssertLinesOfCode(
              @"
                void Foo(Exception ex)
                {
                    goto home; // +1
                    throw ex; // +1
                    home: // +1

                    while (true) // +1
                    {
                        continue; // +1
                        break; // +1
                    }
                    return; // +1
                }",
                4, 5, 6, 8, 10, 11, 13);
        }

        [TestMethod]
        public void Test_09_Yields()
        {
            AssertLinesOfCode(
              @"
               using System;
               using System.Collections.Generic;
               using System.Linq;

               namespace Test
               {
                   class Program
                   {
                       IEnumerable<string> Foo()
                       {
                           yield return ""; // +1
                           yield break; // +1
                       }
                   }
               }",
               12, 13);
        }

        [TestMethod]
        public void Test_10_AccessAndInvocation()
        {
            AssertLinesOfCode(
              @"
                static void Main(string[] args)
                {
                    var x = args.Length; // +1
                    args.ToString(); // +1
                }",
                4, 5);
        }

        [TestMethod]
        public void Test_11_Initialization()
        {
            AssertLinesOfCode(
              @"
                static string GetString() => "";

                static void Main()
                {
                    var arr = new object();
                    var arr2 = new int[] { 1 }; // +1

                    var ex = new Exception()
                    {
                        Source = GetString(), // +1
                        HelpLink = ""
                    };
                }",
                7, 11);
        }

        [TestMethod]
        public void Test_12_Property_Set()
        {
            AssertLinesOfCode(
              @"
                class Program
                {
                    int Prop { get; set; }

                    void Foo()
                    {
                        Prop = 1; // + 1
                    }
                }",
                8);
        }

        [TestMethod]
        public void Test_13_Property_Get()
        {
            AssertLinesOfCode(
              @"
                class Program
                {
                    int Prop { get; set; }

                    void Foo()
                    {
                        var x = Prop;
                    }
                }");
        }

        [TestMethod]
        public void Test_14_Lambdas()
        {
            AssertLinesOfCode(
              @"
                using System;
                using System.Linq;
                using System.Collections.Generic;
                class Program
                {
                    static void Main(string[] args)
                    {
                        var x = args.Where(s => s != null) // +1
                            .Select(s => s.ToUpper()) // +1
                            .OrderBy(s => s) // +1
                            .ToList();
                    }
                }",
                9, 10, 11);
        }

        [TestMethod]
        public void Test_15_TryCatch()
        {
            AssertLinesOfCode(
              @"
                using System;
                class Program
                {
                    static void Main(string[] args)
                    {
                        try
                        {
                            Main(null);  // +1
                        }
                        catch (InvalidOperationException)
                        {
                        }
                        catch (ArgumentNullException ane) when (ane.ToString() != null) // +1
                        {
                        }
                        finally
                        {
                        }
                    }
                }",
                9, 14);
        }

        [TestMethod]
        public void Test_16()
        {
            AssertLinesOfCode(
             @"using System;
               public void Foo(int x) {
               int i = 0; if (i == 0) {i++;i--;} else // +1
               { while(true){i--;} } // +1
               }",
               3, 4);
        }

        [TestMethod]
        public void Test_17()
        {
            AssertLinesOfCode(
             @"
                static void Main(string[] args)
                {
                    do // +1
                    {

                    }

                    while
                    (
                    true
                    )
                    ;
                }",
                4);
        }

        [TestMethod]
        public void Test_18_ExcludeFromTestCoverage()
        {
            AssertLinesOfCode(
              @"using System.Diagnostics.CodeAnalysis;
                namespace project_1
                {
                    public class ComplicatedCode
                    {
                        [ExcludeFromCodeCoverage]
                        public string ComplexFoo()
                        {
                            string text = null;
                            return text.ToLower();
                        }

                        [SomeAttribute]
                        public string ComplexFoo2()
                        {
                            string text = null;
                            return text.ToLower(); // +1
                        }
                    }
                }",
                17);
        }

        [TestMethod]
        public void Test_19_ExcludeFromTestCoverage_Variants()
        {
            var attributeVariants = new List<string>
            {
                "ExcludeFromCodeCoverage",
                "ExcludeFromCodeCoverage()",
                "ExcludeFromCodeCoverageAttribute",
                "ExcludeFromCodeCoverageAttribute()",
                "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute()"
            };

            attributeVariants.ForEach(attr => AssertLinesOfCode(GenerateTest(attr)));
        }

        private static string GenerateTest(string attribute)
        {
            return @"using System.Diagnostics.CodeAnalysis;
                namespace project_1
                {
                    public class ComplicatedCode
                    {
                        [" + attribute + @"]
                        public string ComplexFoo()
                        {
                            string text = null;
                            return text.ToLower();
                        }
                    }
                }";
        }

        [TestMethod]
        public void Test_20_ExcludeClassFromTestCoverage()
        {
            AssertLinesOfCode(
              @"
                using System;
                [ExcludeFromCodeCoverage]
                class Program
                {
                    static void Main(string[] args)
                    {
                        Main(null);
                    }
                }");
        }

        [TestMethod]
        public void Test_21_ExcludeStructFromTestCoverage()
        {
            AssertLinesOfCode(
              @"namespace project_1
                {
                    [ExcludeFromCodeCoverage]
                    struct Program
                    {
                        static void Foo()
                        {
                            Foo();
                        }
                    }
                }");
        }

        [TestMethod]
        public void Test_22_ExcludePropertyFromTestCoverage()
        {
            AssertLinesOfCode(
              @"[ExcludeFromCodeCoverage]
                class Program
                {
                    int FooProperty
                    {
                        get
                        {
                            return 1;
                        }
                    }
                }");
        }
    }
}
