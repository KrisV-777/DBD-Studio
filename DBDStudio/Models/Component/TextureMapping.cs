namespace DBDStudio.Models.Component.Textures
{
    public sealed class TextureMapping(string vanillaTexture, string replacementTexture, string? absolutePath)
        : ModelBase, IEquatable<TextureMapping>
    {
        // TOOD: absolutePath can be invalid, should have some UI clue that the owning pack cannot be exported until fixed
        // TODO: ^^^ needs a feature to edit absolute path

        #region Fields

        private string _vanillaTexture = vanillaTexture;
        private string _replacementTexture = replacementTexture;
        private string? _absolutePath = absolutePath;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the vanilla texture that this mapping corresponds to. Relative to the vanilla texture directory.
        /// </summary>
        public string VanillaTexture
        {
            get => _vanillaTexture;
            set => SetProperty(ref _vanillaTexture, value);
        }

        /// <summary>
        /// Gets or sets the name of the replacement texture that this mapping corresponds to. Relative to the DBD profile texture directory.
        /// </summary>
        public string ReplacementTexture
        {
            get => _replacementTexture;
            set => SetProperty(ref _replacementTexture, value);
        }

        /// <summary>
        /// Gets or sets the source file of the texture mapping, indicating where the mapping is located on the filesystem.
        /// </summary>
        public string AbsolutePath
        {
            get => _absolutePath ?? string.Empty;
            set => SetProperty(ref _absolutePath, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a clone of the current <see cref="TextureMapping"/> instance.
        /// </summary>
        /// <returns>A new <see cref="TextureMapping"/> instance that is a copy of the current instance.</returns>
        public TextureMapping Clone() => (TextureMapping)MemberwiseClone();

        #endregion
        public bool Equals(TextureMapping? other) =>
            other is not null && string.Equals(_vanillaTexture, other._vanillaTexture, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as TextureMapping);

        public override int GetHashCode() => HashCode.Combine(
            _vanillaTexture?.ToLowerInvariant(),
            _replacementTexture?.ToLowerInvariant(),
            _absolutePath?.ToLowerInvariant());
    }
}
