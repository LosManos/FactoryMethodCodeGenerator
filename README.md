# Readme for FactoryMethodCodeGenerator

This project is a personal (but not private) project for exploring Roslyn source generator.

## First goal - Achieved

The first task is to find this `record`
```
[Dto]
public partial record MyDto{
  public string MyProperty{ get; init; }
}
```
and create
```
public partial record MyDto{
  private MyDto(string MyProperty){
    this.MyProperty = MyProperty;
  }
  public static MyDto Create(string MyProperty){
    return new MyDto(MyProperty);
  }
}
```

The second task is to find these records
```
[Dto]
public partial record SourceType{...}
[Dto]
public partial record TargetType{...}
```
and create a mapper like
```
public static class Mapper{
    public TargetType Map(SourceType sourceType){...}
}
```


## Future goals

### Constructor creator
* Take any type of property. All value type primitives. Then reference types and deeper structures.
* Decide visibility (public/internal). Default is internal I guess. Set through Attribute? Use properties' visibilities?
* Decide whether the constructor should be private or not. Can also be settable - but with the "correct" default.
* Handle types that are inside other classes or records. By the time of writing they have to be directly in a namespace.

### Mapper
* User settable names.
* Is the present static construction the "correct" solution?
* 
## Far future goals

* Make it simply usable for anyone. Like Automapper.
