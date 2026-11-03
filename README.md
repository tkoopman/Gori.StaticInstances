# Gori.StaticInstances

A Roslyn source generator that implements the static instance and/or singleton pattern via attributes, 
with support for Lazy and Weak references (Thread Safe or Unsafe) for instance initialization.

## `[StaticInstance]`

Place on a constructor to generate a static instance using that constructor and the supplied arguments. 
Multiple attributes can be applied to the same constructor. 
If constructor has parameters then you must add matching arguments to the `[StaticInstance]` attribute, 
before [named attribute options](#attribute-options).

```csharp
[Singleton]
public partial class DbContext
{
	private DbContext() { }

	[StaticInstance("localhost", 5432, Name = "Development")]
	[StaticInstance("staging-host", 5432, Name = "Staging")]
	private DbContext(string host, int port) { }
}

// Generated:
// public static DbContext Instance     { get; } = new();
// public static DbContext Development  { get; } = new("localhost", 5432);
// public static DbContext Staging      { get; } = new("staging-host", 5432);
```

---

## `[Singleton]`

Is a special type of StaticInstance that is placed on a class and always uses the default constructor.
Only a single Singleton attribute can exist per class. Class can still use StaticInstance attributes along side Singleton.

```csharp
[Singleton]
public partial class MyService { }

// Generated:
// private MyService() { }
// public static MyService Instance { get; } = new();
```

---

## Attribute Options

Options for both StaticInstance and Singleton attributes are mostly the same. To avoid confusion with passing constructor parameters to `[StaticInstance]`, all attribute options are named.

| Attribute      | Option        | Type                                   | Default      | Description                                                                  |
| -------------- | ------------- | -------------------------------------- | ------------ | ---------------------------------------------------------------------------- |
| StaticInstance | `Name`        | `string`                               | None         | Mandatory. Name of the generated static instance                             |
| Singleton      | `Name`        | `string`                               | `"Instance"` | Optional. Name of the generated static instance                              |
| *              | `Type`        | [`InstanceType`](#option-instancetype) | `Property`   | Type of static instance to create                                            |
| *              | `Internal`    | `bool`                                 | `false`      | Generates with access modifier of `internal` instead of the default `public` |
| Singleton      | `Constructor` | [`Constructor`](#option-constructor)   | `Auto`       | Options for creating private default constructor if missing                  |


> All names across `[Singleton]` and `[StaticInstance]` attributes on the same class must be unique.

All instances for a type, including those defined across partial declarations, are generated together in one source file.
Duplicate names produce an error; only one member is emitted, with the singleton taking precedence over static instances.

### Option: InstanceType

Controls what type of static instance is created. If class has multiple static instances, each can be of different types.

| InstanceType | Description                                                                                                  |
| ------------ | ------------------------------------------------------------------------------------------------------------ |
| Property     | Default. Creates a static read-only property                                                                 |
| Field        | Creates a static read-only field                                                                             |
| Lazy         | Creates a static read-only property backed by `Lazy{T}` field                                                |
| Weak         | Creates a static read-only property backed by `WeakReference{T}` field. This will create thread-safe version |
| WeakLockless | Creates a static read-only property backed by `WeakReference{T}` field                                       |

### Option: Constructor

Controls when `[Singleton]` should create a private default constructor for the class.

| Constructor | Description                                                                                     |
| ----------- | ----------------------------------------------------------------------------------------------- |
| Auto        | Default. Will create constructor if class does not contain ANY existing explicit constructor(s) |
| On          | Will create constructor if class does not contain an explicit default constructor               |
| Off         | Will never create a constructor                                                                 |

---

## Analyzer IDs

| Rule ID | Severity | Description                                                                                                                                                 |
| ------- | -------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| GSSI001 | Error    | Class is missing the partial modifier.                                                                                                                      |
| GSSI002 | Error    | `[Singleton]` used but no default constructor. Will not be raised if `Constructor = On`.                                                                    |
| GSSI003 | Info     | `[Singleton]` used on class with existing default constructor that is not private or protected.                                                             |
| GSSI004 | Error    | Name contains invalid characters.                                                                                                                           |
| GSSI005 | Error    | Multiple attributes on class with identical names.                                                                                                          |
| GSSI006 | Warning  | Two or more attributes added to the same constructor, with the same arguments.                                                                              |
| GSSI007 | Error    | Number of arguments defined on `[StaticInstance]` does not match the number of parameters on the constructor.                                               |
| GSSI008 | Warning  | An argument type added to `[StaticInstance]` is of a different type defined for the parameter on the constructor. Will attempt to convert.                  |
| GSSI009 | Info     | An argument added to `[StaticInstance]` is null but is mapped to a value type parameter on the constructor. Will use default value.                         |
| GSSI010 | Error    | `[method: StaticInstance]` added to primary constructor. Due to [Roslyn issue #80511](https://github.com/dotnet/roslyn/issues/80511) this is not supported. |
| GSSI011 | Error    | The source generator requires at least C# 11.0, or C# 13.0 when targeting .NET 9 or higher. Increase the LangVersion property in your project file.         |

---
