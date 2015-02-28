[![Build Status](https://travis-ci.org/forgedsoftware/measurementcs.svg?branch=master)](https://travis-ci.org/forgedsoftware/measurementcs)

measurementcs
=============

A measurement library in c# for handling and manipulating quantities.

## Documentation

This documentation gives a brief overview of what measurement can do...

### Getting Started

[Download and install](http://measurementcs.com/) the Measurement.dll library. Then, add the library to your C# project within Visual Studio.

### Basic Usage
````csharp
using ForgedSoftware.Measurement;

var quantity = new Quantity(5, "minutes");
Console.WriteLine(quantity.ToString()); // 5 min

quantity = quantity.Convert("seconds");
Console.WriteLine(quantity.Value); // 300

quantity = quantity.Add(30);
Console.WriteLine(quantity.Value); // 330
Console.WriteLine(quantity.ToString()); // 330 s

quantity = quantity.Convert("minutes");
Console.WriteLine(quantity.Value); // 5.5
````
### Quantities
A Quantity is a representation of a physical measurement or quantity. A quantity has a Value that represents its size and a set of Dimensions that provide context to that size. For example, a quantity might be "10 metres", having a Value of 10 and a single [Dimension](#dimensions) that represents a metre, which is a Unit of length.

````csharp
var quantity = new Quantity(10, "metres");
Console.WriteLine(quantity.Value); // 10
Console.WriteLine(quantity.Dimensions.Count); // 1
Console.WriteLine(quantity.Dimensions[0].Unit.Key); // "metre"
````
#### Multiple Dimensions

More complex measurements may have more [Dimensions](#dimensions). Here is a example of speed, which is measured in metres per second.
````csharp
var quantity = new Quantity(10, new List<Dimension> {
    new Dimension("metre", 1),
    new Dimension("second", -1)
});
Console.WriteLine(quantity.ToString()); // 10 m·s⁻¹
Console.WriteLine(quantity.Dimensions.Count); // 2
````

#### Derived Dimensions

Alternatively to the example above, speed itself can be used. Speed is considered independently of its constituent base dimensions as a single derived dimension in its own right.
````csharp
var quantity = new Quantity(10, "metrePerSecond");
Console.WriteLine(quantity.ToString()); // 10 m/s
Console.WriteLine(quantity.Dimensions.Count); // 1

MeasurementCorpus.Options.AllowDerivedDimensions = false;
quantity.Simplify();

Console.WriteLine(quantity.ToString()); // 10 m·s⁻¹
Console.WriteLine(quantity.Dimensions.Count); // 2
````

#### Mathematical Operations

Built into quantity are the means to carry out mathematical operations between
quantities and a scalar value and between two quantities. Additionally,
standard mathematical functions have been built in.

````csharp
var quantity = new Quantity(12, "ohms");

// General Operations
quantity = quantity.Multiply(2); // 24 ohms
quantity = quantity.Subtract(3); // 21 ohms
quantity = quantity.Add(7, "ohms"); // 28 ohms
quantity = quantity.Subtract(2, "reciprocalSiemens"); // 26 ohms
// Note: reciprocal siemens are a old non standard unit that are equivalent to 1 ohm.
// The provided quantity is automatically converted.

// More Operations
quantity = quantity.Add(2.2).Floor(); // 28 ohms
quantity = quantity.Multiply(-2).Abs(); // 56 ohms
quantity = quantity.Add(8).Sqrt(); // 8 ohms
// Also Ceiling, Pow, Round, Max, Min
````

#### Convert
In some of the examples above we covered conversion. Converting is simply converting the unit of one dimension to another unit of the same system,
````csharp
new Quantity(5, "minutes").Convert("seconds").ToString(); // 300 s
````
Conversion can be used in quantities with multiple dimensions as well.
````csharp
var quantity = new Quantity(5, new List<Dimension> {
    new Dimension("metre", 1),
    new Dimension("second", -1)
});
quantity = quantity.Convert("inches"); // Convert the length based dimension(s) into inches
Console.WriteLine(quantity.ToString()); // 196.8505 in·s⁻¹
````

#### Simplify
Simplify allows us to attempt to simplify into the simplest possible set of [Dimensions](#dimensions) for a given Quantity.

````csharp
var quantity = new Quantity(30, new List<Dimension> {
    new Dimension("metre", 2),
    new Dimension("inch"),
    new Dimension("foot", -1),
    new Dimension("second"),
});
quantity = quantity.Simplify();
Console.WriteLine(quantity.ToString()); // 2.5 m²·s
````

#### Prefixes
Prefixes provide a way of handling large or small values by the SI measurement system. For example: kilo, mega, giga, tera, peta. By default, Quantities handle prefixes automatically and the prefixes are part of [Dimensions](#dimensions). Ideally, only one Dimension in a quantity should have a prefix.

````csharp
var quantity = new Quantity(5, new Dimension("metre", "kilo"));
Console.WriteLine(quantity.ToString()); // 5 km
````

### Dimensions
Dimensions have been used extensively above, but understanding dimensions are important to get the most out of measurement. A Dimension is a construct that represents how one factor of the Quantity corresponds to the physical world. For example a Quantity may have the Dimension of metres. This is a very simple case, because a full Dimension is defined with three properties - a Unit, a Power, and a Prefix. In the case of a metre it has the Unit of metres, a Power of 1 (the default), and a no Prefix (the default).

- Unit - A representation of a physical dimension (we call these DimensionDefinitions) based on a specific
system. For example, feet and inches are representations of the length dimension in the imperial measurement system.
- Power - The number of times the unit's dimension is applied. By default this can be one. In the case of metres, if the power becomes 2 it is area and if it is 3 it becomes volume. Conversely powers can be negative, an inverse. For example, seconds with a negative one power become frequency. A dimension with a
power of 0 has no effect on the quantity and will be removed if simplified.
- Prefix - A prefix is a specific augmentation of a dimension for the SI system to facilitate the representation of large or small values. For example a quantity of 1 metre could alternatively be represented as 100 centimetres where the centi- prefix is this part of the dimension.

### Generic Quantities
Generic quantities allow us to create quantities with values based on Vectors, Uncertainties, and Fractional Numbers.

````csharp
var quantity = new Quantity<Fraction>(new Fraction(11, 7), "seconds");
Fraction frac = quantity.Value;
Console.WriteLine(frac.Denominator); // 7
````
The different types available are:
 - DoubleWrapper - a helper class to representing doubles, identical to using basic Quantity
 - Fraction - for representing fractional values
 - Uncertainty - for representing values with a positive and/or negative uncertainty. Additionally it automatically handles relative vs absolute uncertainties throughout the calculation.
 - Vector2 - A 2-dimensional vector value
 - Vector3 - A 3-dimensional vector value
 - Vector4 - A 4-dimensional vector value

### Measurement Corpus
The measurement corpus is the central repository for helper functions, find, options, and the data
around units, systems, dimensions, and prefixes that make measurement tick. This data can be
found raw in the [measurementcommon repo](https://github.com/forgedsoftware/measurementcommon).

#### Systems
A system, internally using the type MeasurementSystem, is a system of units like SI, metric, english units, or imperial. The systems are formed into a hierarchical tree with supersets above subsets of
units.
````csharp
List<MeasurementSystem> allSystems = MeasurementCorpus.AllSystems; // All systems
List<MeasurementSystem> rootSystem = MeasurementCorpus.RootSystems; // Root Systems of system tree.
````

#### Dimension Definitions
A dimension is either a type of measurement in the phyical world or the specific instance of that type. We seperate these concepts into a DimensionDefinition and a [Dimension](#dimensions) (as used by Quantity) repectively. A dimension definition includes things like length, volume, electicDipoleMoment and time. 
````csharp
List<DimensionDefinitions> allDimensions = MeasurementCorpus.Dimensions; // All dimension definitions
````

#### Units
A Unit is a specific measurement of a DimensionDefinition and belongs to one or more MeasurementSystems. For example an inch is a Unit it has a DimensionDefinition of length and MeasurementSystems of englishUnits and imperial (being used by both). Note that these systems also have other length units as well - such as the foot and yard.

DimensionDefinitions also have a BaseUnit, a unit that is used to convert between all other units belonging
to that dimension using a common conversion point. We use a metric set of units to provide this.

````csharp
List<Unit> lengthUnits = MeasurementCorpus.FindDimension("length").Units; // All length units
Unit baseLengthUnit = MeasurementCorput.FindDimension("length").BaseUnit; // metre - the base unit of length
List<Unit> allUnits = MeasurementCorpus.AllUnits; // All units in all dimensions
````

#### Prefixes
A Prefix is a specific augmentation to a Unit (combined in a Dimension) to provide a scale to clearly
represent large and small values. Primarily this is a feature of the SI MeasurementSystem. In addition a specific subset are provided to handle base-2 values in measuring binary information and another subset are provided of prefixes that have been proposed but are not SI-approved.

````csharp
List<Prefix> allPrefixes = MeasurementCorpus.Prefixes; // All prefixes
````

#### Find
Find searches through the different parts of the MeasurementCorpus to find any of the following types MeasurementSystem, DimensionDefinition, Unit, or Prefix based on a full or partial string match. Additionally, find will either pull out all results or just the top result of the find. The find works through the relevant string properties of the entity including the Key, Names, Plural, Symbols etc. By default they ignore case.

````csharp
Unit foundUnit = MeasurementCorpus.FindUnit("metre"); // metre (length)
foundUnit = MeasurementCorpus.FindUnit("\""); // inch (length)
foundUnit = MeasurementCorpus.FindUnit("min"); // minute (time)
foundUnit = MeasurementCorpus.FindUnitPartial("min", "planeAngle"); // minute (planeAngle)

List<MeasurementSystem> systems = MeasurementCorpus.FindSystemsPartial("metric"); // All systems called 'metric' somewhere

DimensionDefinition foundDim = MeasurementCorpus.FindDimension("length"); // length
List<DimensionDefinitions> dims = MeasurementCorpus.FindDimensionsPartial("electric");
// Finds electricCurrent, electricCharge, electricResistance, and electricDipoleMoment

Prefix foundPrefix = FindPrefix("centi"); // centi- prefix
foundPrefix = FindPrefixPartial("Mi"); // micro- prefix
foundPrefix = FindPrefixPartial("Mi", ignoreCase: false); // mebi- prefix (for data)
````

#### Options
Provides a series of global options used across Measurement.
````csharp
MeasurementOptions options = MeasurementCorpus.Options;
````
They can be reset to their defaults using...
````csharp
MeasurementCorpus.ResetToDefaultOptions();
````

## License et al
This is licensed under MIT. For more information feel free to check out the source code.