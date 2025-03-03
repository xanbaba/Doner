using System.Text.Json.Serialization;

namespace Doner.Features.ReelsFeature.Elements;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Checkbox), typeDiscriminator: "checkbox")]
public abstract class Element;