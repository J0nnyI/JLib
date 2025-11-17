# Refactoring: Create Method
Idea: move the initialization from the ctor into a thread-safe async create method 
## Advantages: 
- create process can be parallelized
- frees up the ctor

## 

## Challenges:
- the create method must be called outside the ctor
- the create method may be called outside the resolver

## Ideas:
- The Service-Resolver may use a factory, which first calls the ctor and then calls the create method.
  - Considerations 
    - how do we make sure that the dependencies are initialized as well?
      - the dependencies would use the same service resolver variant, guaranteeing they are created before leaving 
        their resolver  
    - how do we ensure thread safety? wa should leverage the Dependency injection system for that.
    - how do we parallelize the entity genxeration?
      - we could make the serviceType ValueTask\<TService> and return the active task
        - sounds hacky
      - we could add a GetDataPackageAsync<>() method to a specialized service provider
    - how do we make sure, that dependent packages are created after their dependencies?
    - how do we make sure, that all required packages are created when all is done?
    - how do we make sure, that the dbContext does not collide with itself during parallel write?
      -  
    - how do we keep the ids consistent, if their access order is no longer deterministic? 
 

# Pattern: CreateEntityMethod