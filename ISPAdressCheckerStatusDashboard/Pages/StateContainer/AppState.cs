namespace BlazorTestProjects.BlazorConcepts.StateContainer

{
    public class AppState
    {
        public string SelectedColour { get; private set; }

        public event Action OnChange;

        public List<string> ColorList { get; private set; } = new();

        public void SetColour(string colour)
        {
            SelectedColour = colour;
            ColorList.Add(colour);
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
