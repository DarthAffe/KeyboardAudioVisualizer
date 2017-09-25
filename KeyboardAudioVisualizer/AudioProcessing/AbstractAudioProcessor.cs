namespace KeyboardAudioVisualizer.AudioProcessing
{
    public abstract class AbstractAudioProcessor : IAudioProcessor
    {
        #region Properties & Fields

        public bool IsActive { get; set; } = true;

        #endregion

        #region Methods

        public abstract void Initialize();

        public abstract void Update();

        public virtual void Dispose() { }

        #endregion
    }
}
