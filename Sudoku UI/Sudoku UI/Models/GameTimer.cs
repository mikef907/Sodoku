using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sudoku_UI.Models
{
    public class GameTimer: IDisposable
    {
        private bool disposedValue;

        private bool isRunning { get; set; }
        private Action action { get; set; }

        public GameTimer(Action action) {
            this.action = action;
        }

        public void StopTimer() => isRunning = false;
        public void StartTimer() => isRunning = true;
        public void InitTimer() { 
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (action == null) return false;
                else if(isRunning) {
                    Device.BeginInvokeOnMainThread(action);
                }
                return isRunning;
            });        
        }
      

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this.action = null;
                    isRunning = false;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GameTimer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}