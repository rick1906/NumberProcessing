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
