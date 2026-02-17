using System.Media;
using NAudio.Wave;

namespace PacmanGameProject.Game.Services;

// service responsavel pelos sons
public class GameAudioService : IDisposable
{
    private SoundPlayer _eatSound; // som comer
    private SoundPlayer _deathSound; // som morrer
    
    private bool _isChomping = false; // ta comendo?
    private DateTime _lastPelletTime = DateTime.MinValue;  // ultima vez pellet foi comido
    private const int AUDIO_TIMEOUT_MS = 300; // tempo max entre pellets para parar som
    private IWavePlayer _bgmOutput; // player musica backgound
    private AudioFileReader _bgmReader; // leitor arquivo musica

    // carrega os efeitos sonoros
    public void SetupAudio()
    { 
        string baseDir = AppContext.BaseDirectory;

        // carrega som pellet
        string eatPath = Path.Combine(baseDir, "Assets", "sounds", "pacman_chomp.wav");
        if (File.Exists(eatPath))
        { 
            _eatSound = new SoundPlayer(eatPath);
            _eatSound.Load();
        }

        // carrega som morte
        string deathPath = Path.Combine(baseDir, "Assets", "sounds", "pacman_death.wav");
        if (File.Exists(deathPath))
        { 
            _deathSound = new SoundPlayer(deathPath);
            _deathSound.Load();
        }
    }

    // carrega musica de fundo
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
     
             _bgmOutput.PlaybackStopped += OnPlaybackStopped; // reinicia musica quando acaba
     
             _bgmOutput.Play();
         }

     // reinicia musica quando terminar
    private void OnPlaybackStopped(object sender, StoppedEventArgs e)
    {
        if (_bgmReader == null || _bgmOutput == null) return;

        _bgmReader.Position = 0;
        _bgmOutput.Play();
    }
         
    // chama qnd pacman come pellet
    public void PelletEaten()
    {
        _lastPelletTime = DateTime.Now;
     
        if (!_isChomping && _eatSound != null) 
        {
            _isChomping = true;
            _eatSound.PlayLooping();
        }
    }
      // som morte
    public void PlayDeath()
    {
        _bgmOutput?.Stop(); 
        _deathSound?.Play();
    }
    
     // att estado som mastigação (por enqnt)
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

    // para todos os sons
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

    // libera função de audio
     public void Dispose()
     {
         StopAll();
     }
}

