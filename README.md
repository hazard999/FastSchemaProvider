#FastSchemaProvider 
FastSchemaProvider is an ADO based schema provider trying to get the schema as fast as possible
Additional features
Serialization thru ServiceStack.Text  
Schema upgrade from provided schema
##Genesis
The initial code was taken from Mark Rendle (https://github.com/markrendle) and  Richard Hopton (https://github.com/richardhopton)  
Thank you Mark and Richard
##Optimizations
New Queries + wider structure of schema
##Requirements
NLog (for logging (not used so far)) (via NuGet)  
NUnit for testing (via NuGet)  
ServiceStack.Text for serialization  
iAnwhere.Data.SQLAnywhere.v4.0 for console application