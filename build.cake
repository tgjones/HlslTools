var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Build")
  .Does(() =>
{
  MSBuild("./src/ShaderTools.EditorServices.sln", settings =>
    settings.SetConfiguration(configuration));
});

Task("Default")
  .IsDependentOn("Build");

RunTarget(target);