# Readme for FactoryMethodCodeGenerator

This project is a personal (not private) project for exploring Roslyn source generator.

## First goal

The first task is to find this `record`
```
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
  public static MyDto(string MyProperty){
    return new MyDto(MyProperty);
  }
}
```

## Future goals

* Take any type of property.
* Decide visibility (public/internal). Default is internal I guess. Set through Attribute? Use properties' visibilities?
* Decide whether the constructor should be private or not.

## Far future goals

* Create a mapper. E.g.:`var result = Map.ToResult(myRecord);` or `var result = myrecord.ToResult();`
