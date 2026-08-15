namespace DBDStudio.Models.Textures
{
    public sealed class TextureMapping(string vanillaTexture, string replacementTexture, string? absolutePath) : ModelBase
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
        /// Initializes a new instance of the <see cref="TextureMapping"/> class with the specified vanilla texture, replacement texture, and source file.
        /// </summary>
        /// <returns>A new <see cref="TextureMapping"/> instance that is a copy of the current instance.</returns>
        public TextureMapping Clone() => (TextureMapping)MemberwiseClone();

        #endregion

        #region Equality

        public bool Equals(TextureMapping? other) => other is not null && _vanillaTexture == other._vanillaTexture;
        public override bool Equals(object? obj) => obj is TextureMapping other && Equals(other);
        public static bool operator ==(TextureMapping? left, TextureMapping? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(TextureMapping? left, TextureMapping? right) => !(left == right);
        public override int GetHashCode() => _vanillaTexture.GetHashCode();
        public int CompareTo(TextureMapping? other) => other is null ? 1 : _vanillaTexture.CompareTo(other._vanillaTexture, StringComparison.OrdinalIgnoreCase);
        public static bool operator <(TextureMapping? left, TextureMapping? right) => left is null ? right is not null : left.CompareTo(right) < 0;
        public static bool operator >(TextureMapping? left, TextureMapping? right) => left is not null && left.CompareTo(right) > 0;
        public static bool operator <=(TextureMapping? left, TextureMapping? right) => !(left > right);
        public static bool operator >=(TextureMapping? left, TextureMapping? right) => !(left < right);

        #endregion
    }
}
