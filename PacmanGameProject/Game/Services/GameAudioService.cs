using System.Media;
using NAudio.Wave;

namespace PacmanGameProject.Game.Services;

public class GameAudioService : IDisposable
{
    private SoundPlayer _eatSound;
    private SoundPlayer _deathSound;
    
    private bool _isChomping = false;
    private DateTime _lastPelletTime = DateTime.MinValue;
    private const int AUDIO_TIMEOUT_MS = 250;
    private IWavePlayer _bgmOutput;
    private AudioFileReader _bgmReader;

    public void SetupAudio()
    { 
        string baseDir = AppContext.BaseDirectory;

        string eatPath = Path.Combine(baseDir, "Assets", "sounds", "pacman_chomp.wav");
        if (File.Exists(eatPath))
        { 
            _eatSound = new SoundPlayer(eatPath);
            _eatSound.Load();
        }

        string deathPath = Path.Combine(baseDir, "Assets", "sounds", "pacman_death.wav");
        if (File.Exists(deathPath))
        { 
            _deathSound = new SoundPlayer(deathPath);
            _deathSound.Load();
        }
    }

      public void SetupBackgroundMusic()
         {
             StopAll(); 
     
             string bgmPath = Path.Combine(AppContext.BaseDirectory, "Assets", "sounds", "pacman-soundtrack.mp3");
     
             if (!File.Exists(bgmPath)) return;
     
             _bgmReader = new AudioFileReader(bgmPath)
             {
                 Volume = 0.15f
             };
     
             _bgmOutput = new WaveOutEvent();
             _bgmOutput.Init(_bgmReader);
     
             _bgmOutput.PlaybackStopped += OnPlaybackStopped;
     
             _bgmOutput.Play();
         }

         private void OnPlaybackStopped(object sender, StoppedEventArgs e)
         {
             if (_bgmReader == null || _bgmOutput == null) return;

             _bgmReader.Position = 0;
             _bgmOutput.Play();
         }
         
      public void PelletEaten()
         {
             _lastPelletTime = DateTime.Now;
     
             if (!_isChomping && _eatSound != null)
             {
                 _isChomping = true;
                 _eatSound.PlayLooping();
             }
         }
      
         public void PlayDeath()
         {
             _bgmOutput?.Stop(); 
             _deathSound?.Play();
         }
     
     public void Update()
     {
         if (!_isChomping) return;

         var delta = DateTime.Now - _lastPelletTime;
         if (delta.TotalMilliseconds > AUDIO_TIMEOUT_MS)
         {
             _eatSound?.Stop();
             _isChomping = false;
         }
     }

     public void StopAll()
     {
         if (_bgmOutput != null)
         {
             _bgmOutput.PlaybackStopped -= OnPlaybackStopped;
             _bgmOutput.Stop();
             _bgmOutput.Dispose();
             _bgmOutput = null;
         }

         if (_bgmReader != null)
         {
             _bgmReader.Dispose();
             _bgmReader = null;
         }

         _eatSound?.Stop();
     }

     public void Dispose()
     {
         StopAll();
     }
}

