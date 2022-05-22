# NumberProcessing
A C# library about numbers with uncertainty and metric units. Provides a `AdvancedNumber` struct to store a scientific number representation like "1.234(5)×10<sup>-6</sup>" with methods to perform arithmetic operations, parsing, etc.

A `NumberProcessing.Units` project provides a `NumberContainer` class to store a number with a metric unit, like "1.234(5)×10<sup>-6</sup> s", and perform certain operations with it. It also supports storing value approximations like "~1.234" or "<1.234" and ranges "1.2-1.4".

This classes can be used to store and manage values from scientific articles or databases without losing nessesary details.

## Examples for AdvancedNumber
#### Creating and printing AdvancedNumber
`````cs
AdvancedNumber n1 = new AdvancedNumber(12.34m, 0.25m);
AdvancedNumber n2 = new AdvancedNumber(12.34E-8, 0.25E-8);
AdvancedNumber n3 = new AdvancedNumber(12.34m, 0.25m, 8);
AdvancedNumber n4 = new AdvancedNumber("12.34(25)x10E8");
Console.WriteLine(n1.ToString());
Console.WriteLine(n2.ToString());
Console.WriteLine(n3.ToString());
Console.WriteLine(n4.ToString());
`````
Outputs:
````
12.34 (25)
1.234 (25) × 10E-7
1.234 (25) × 10E9
1.234 (25) × 10E9
````
#### Different formatting
````cs
AdvancedNumber n1 = new AdvancedNumber(12.34m, 0.25m);
AdvancedNumber n2 = new AdvancedNumber(12.34E-8, 0.25E-8);
AdvancedNumber n3 = new AdvancedNumber(1.2345m, 0.752m, -2);
Console.WriteLine(n1.ToString());
Console.WriteLine(n1.ToString(false));
Console.WriteLine(n2.ToString(true, ',', AdvancedNumberFormat.MultiplyDot));
Console.WriteLine(n2.ToString(false, ',', 'x'));
Console.WriteLine(n3.ToString(true));
Console.WriteLine(n3.ToString(new AdvancedNumberFormat() {
    MaxErrorDigits = 3,
    MaxErrorValue = 999,
    MinPowerOf10 = 1,
}));
````
Outputs:
````
12.34 (25)
12.34 ± 0.25
1,234 (25) · 10E-7
(1,234 ± 0,025) x 10E-7
0.012 (8)
1.234 (752) × 10E-2
````
#### Arithmetic operations
````cs
AdvancedNumber n1 = new AdvancedNumber(12.34m, 0.15m);
AdvancedNumber n2 = new AdvancedNumber(99.58m, 0.09m);
AdvancedNumber n3 = new AdvancedNumber(12.34m, 0.25m, 31);
AdvancedNumber n4 = new AdvancedNumber(5.0000m, 0.0005m, -44);
Console.WriteLine((n1 + n2).ToString());
Console.WriteLine((n1 - n2).ToString());
Console.WriteLine((n1 * n2).ToString());
Console.WriteLine((n1 / n2).ToString());
Console.WriteLine((n1 * 2.5).ToString());
Console.WriteLine((n3 * n4).ToString());
Console.WriteLine((n3 / n4).ToString());
````
Outputs:
````
111.92 (17)
-87.24 (17)
1229 (15)
0.1239 (15)
30.85 (38)
6.17 (13) × 10E-12
2.47 (5) × 10E75
````

## Examples for NumberContainer
#### Working with metric units
````cs
NumberContainer nc1a = new NumberContainer(60, "s");
NumberContainer nc2a = new NumberContainer(5, "min");
NumberContainer nc1b = new NumberContainer(3, "kg");
NumberContainer nc2b = new NumberContainer(500, "g");
AdvancedNumber n1a = nc1a.GetNormalizedTimeValue();
AdvancedNumber n2a = nc2a.GetNormalizedTimeValue();
AdvancedNumber n1b = nc1b.GetNormalizedValue("g");
AdvancedNumber n2b = nc2b.GetNormalizedValue("g");
AdvancedNumber x = n1b + n2b;
Console.WriteLine((n1a + n2a).ToString() + " s");
Console.WriteLine((n1b + n2b).ToString() + " g");
````
Outputs:
````
360 s
3500 g
````
#### Parsing & using different approximation types
````cs
NumberContainer nc1 = NumberContainer.Parse("33.44(1) kg");
NumberContainer nc2 = NumberContainer.Parse("~10.5 min");
NumberContainer nc3 = NumberContainer.Parse("3-5 h");
NumberContainer nc4 = NumberContainer.Parse("<10 g");
Console.WriteLine(nc1.ToString() + " | " + nc1.Modifier + " | " + nc1.Number);
Console.WriteLine(nc2.ToString() + " | " + nc2.Modifier + " | " + nc2.Number);
Console.WriteLine(nc3.ToString() + " | " + nc3.Modifier + " | " + nc3.Number);
Console.WriteLine(nc4.ToString() + " | " + nc4.Modifier + " | " + nc4.Number);
````
Outputs:
````
33.44 (1) kg | Normal | 33.44 (1)
~ 10.5 min | Approximate | 10.5
3 - 5 h | Range | 4 (1)
< 10 g | LessThan | 10
````
