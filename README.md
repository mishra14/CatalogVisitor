# CatalogVisitor

To build & import API DLL nupkg:

C:\Users\t-kaswan\Documents\Visual Studio 2015\Projects\CatalogVisitor\src\NuGet.CatalogVisitor>C:\Users\t-kaswan\Desktop\nuget.exe pack NuGet.CatalogVisitor.csproj -version 1.0.1-beta

Make sure nuget.exe and NuGet.CatalogVisitor.csproj are in the location you are packing the nupkg.

Inside VisualStudio NuGet Package Manager, add a new hard drive source to a new folder you made that you moved the nupkg to.

Now, you should be able to easily import your newly packed nupkg to any new csproj on VS and use the API.
