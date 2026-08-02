using Afterimage.Api.Contracts;
using Afterimage.Domain.Photos;
using Riok.Mapperly.Abstractions;

namespace Afterimage.Api.Mapping;

/// <summary>
/// Compile-time mapping (source-generated) from the domain entity to its DTO.
/// The enum <see cref="Photo.LifecycleState"/> maps to the string
/// <see cref="PhotoDto.Status"/> by name (ADR-0004).
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class PhotoMapper
{
    [MapProperty(nameof(Photo.LifecycleState), nameof(PhotoDto.Status))]
    public partial PhotoDto ToDto(Photo photo);
}
