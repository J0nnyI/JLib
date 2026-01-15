# JLib.Exceptions

Provides a standardized way to create, aggregate and throw exceptions.

[ExceptionBuilder Examples](../Examples/JLib.Exceptions.Examples/ExceptionBuilderExamples)



# Issues with the current implementation

- The JLibException and JLibAggregateException sound similar but represent different UseCases:
    - Exception:
        - JLibException: An exception thrown by the JLib itself
        - JLibAggregateException: extended aggregate exception which can be used by everyone, optimized for better
          readabillity
    - Solutions:
        - Delete the JLibAggregateException and move the utility to an Extension Method
            - Drawback: the ToString has to be overridden manually to get the improved message
        - Rename JlibAggregateException to ExtendedAggregateException
            - Drawback: The name is long and not very descriptive
        - Discard the JLibException as base-class for all exceptions thrown by the JLib
            - Drawback: Exceptions from the JLib can no longer be easily distinguished from other exceptions
                - is this even a reasonable usecase?

