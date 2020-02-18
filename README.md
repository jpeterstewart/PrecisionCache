# PrecisionCache

This cache follows a different philosophy from most of the cache implementations out there. Here's a list of what it is and what it is not:

* It is not a cache "layer", indiscriminately caching things wether they need to be cached or not.
* It is not intended to transparently sit between the consumer and the data source, keeping track of updates and automatically clearing cache content as needed.
* It can be used for web sites, but it can also be used for other types of applications.
* It is not intended to be used where data needs to be cached for use by more than one server.
* There is no complicated configuration, in fact there is no configuration at all.
* It is very small and lightweight and intended to be used at the exact point where caching can improve performance.
* It is instantiated at the spot where the data is retrieved or the result of a complex calculation needs to be cached.
* The item expiration time can be set individually at each spot, from one minute and up.
* It is thread safe.

It is named Precision Cache because it can be precisely tailored for each piece of data. 
(And because there are so many cache implementations out there that it gets very difficult to find an available name...)

## HOW TO USE PRECISIONCACHE

### DECLARATION

Declare a static instance of the class in the object where a cache should be used:

```C#
    private static readonly IPrecisionCache MyLocalCache = new SimpleLocalCache();  
                                                           // default item expiration: 30 m
```
OR		

```C#	
    private static readonly IPrecisionCache MyLocalCache = new SimpleLocalCache(5);  
                                                           // item expiration: 5 m
```
OR 

```C#
    private static readonly IPrecisionCache MyLocalCache = new SimpleLocalCache(5, 10); 
                                                        // item expiration: 5 m, trim expired items every 10 m
```

Avoid caching the same data in multiple locations! 


### SAVE TO CACHE

To save any object to the cache:

```C#
    MyLocalCache.AddOrUpdate(key, value);
```
OR			

```C#
    MyLocalCache.AddOrUpdate(key, value, tags);  // tags are optional
```
Note that the key can easily be created as a composite key:

```C#
    var key = key1 + "|" + key2 + "|" + key3;
```

### RETRIEVAL

To retrieve an object from the cache:

```C#
    if (MyLocalCache.TryGetValue(key, out var valueFromCache) )
```

The cachae would normally be used as follows:

```C#
    object cachedData;
    if (MyLocalCache.TryGetValue(key, out cachedData))
    {
        return cachedData; // use the data
    }
    else
    {
        var myNewData =  ???????????????????;   // retrieve/create object holding the data
        
        MyLocalCache.AddOrUpdate(key, myNewData);  // add to cache

        return myNewData;  // use the data
    }
```
