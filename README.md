# Weswen

Utilities for simple automatic dummy data generation, reflection and Word documents management (`.docx` files).

## Samples

### Data generation

The dummy data is generated using the `Generator` class. Some simple types (such as `int` or `string`) already have defined generation methods. In order to override them, or to make some for custom types, use the method `SetTypeSpec`. Once you want to generate something, you can use helper static methods `One` and `Many` to generate on or more objects of the given type.

``` C#
var randomNumber = Generator.One<int>();
var randomStrings = Generator.Many<string>();
var twoRandomGuids = Generator.Many<Guid>(2);
```

### File management

If you need to work with multiple files which may be in a specific directory, there is a simple class `FileManager` which can do just that and which supports several serializations out of the box (binary, XML, JSON).

### Word documents (`.docx` files) table manager

If you need to work with tables in Word, you can use the `WordManager` that can convert simple models from and to Word tables. To setup the models, all you have to do is decorate the model class with `TableModel` and its properties with `TableCell` attributes. After that, call the appropriate methods on `WordManager` to create Word document with collection of those models or parse a Word document that contains data structured as is defined with the attributes in the table model class.