using System.Media;
using NAudio.Wave;
using System.IO;

namespace PacmanGameProject.Game.Services;

public class GameAudioService : IDisposable
{
    private SoundPlayer _eatSound;       // Chomp
    private SoundPlayer _deathSound;     // Morte
    private SoundPlayer _eatGhostSound;  // NOVO: Comer Fantasma

    private bool _isChomping = false;
    private DateTime _lastPelletTime = DateTime.MinValue;
    private const int AUDIO_TIMEOUT_MS = 300;

    // Player de Música (NAudio)
    private IWavePlayer _bgmOutput;
    private AudioFileReader _bgmReader;

    // Caminhos dos arquivos de música
    private string _normalMusicPath;
    private string _frightenedMusicPath;
    private bool _isFrightenedMusicPlaying = false;

    private bool _isMuted = GlobalSettings.IsMuted;

    public void SetupAudio()
    {
        string baseDir = AppContext.BaseDirectory;
        
        _eatSound = LoadSound(Path.Combine(baseDir, "Assets", "sounds", "pacman_chomp.wav"));
        _deathSound = LoadSound(Path.Combine(baseDir, "Assets", "sounds", "pacman_death.wav"));
        
        _eatGhostSound = LoadSound(Path.Combine(baseDir, "Assets", "sounds", "pacman_eatghost.wav"));

        _normalMusicPath = Path.Combine(baseDir, "Assets", "sounds", "pacman-soundtrack.mp3");
        _frightenedMusicPath = Path.Combine(baseDir, "Assets", "sounds", "pacman_intermission.wav");
    }

    // Helper para carregar SoundPlayer com segurança
    private SoundPlayer LoadSound(string path)
    {
        if (File.Exists(path))
        {
            var sp = new SoundPlayer(path);
            sp.Load();
            return sp;
        }
        return null;
    }

    public void SetupBackgroundMusic()
    {
        // Começa com a música normal
        PlayMusic(_normalMusicPath, 0.15f);
        _isFrightenedMusicPlaying = false;
    }

    public void PlayFrightenedMusic()
    {
        if (_isFrightenedMusicPlaying) return; // Já está tocando

        // Toca a música de Intermission
        PlayMusic(_frightenedMusicPath, 0.15f);
        _isFrightenedMusicPlaying = true;
    }

    public void ResumeNormalMusic()
    {
        if (!_isFrightenedMusicPlaying) return; // Já está tocando a normal

        PlayMusic(_normalMusicPath, 0.15f);
        _isFrightenedMusicPlaying = false;
    }

    // Método genérico para trocar a faixa do NAudio
    private void PlayMusic(string path, float volume)
    {
        // 1. Para e Limpa o anterior
        StopMusic();

        if (!File.Exists(path)) return;

        try
        {
            // 2. Carrega o novo
            _bgmReader = new AudioFileReader(path) { Volume = _isMuted ? 0f : volume };
            _bgmOutput = new WaveOutEvent();
            _bgmOutput.Init(_bgmReader);
            _bgmOutput.PlaybackStopped += OnPlaybackStopped;
            _bgmOutput.Play();
        }
        catch (Exception ex)
        {
            // Log de erro se necessário
            System.Diagnostics.Debug.WriteLine($"Erro ao tocar música: {ex.Message}");
        }
    }

    private void StopMusic()
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
    }

    private void OnPlaybackStopped(object sender, StoppedEventArgs e)
    {
        // Loop simples: Se a música parou (e não foi a gente que parou manualmente), reinicia
        if (_bgmReader != null && _bgmOutput != null)
        {
            _bgmReader.Position = 0;
            _bgmOutput.Play();
        }
    }

    public void PlayEatGhost()
    {
        _eatGhostSound?.Play();
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
        StopMusic(); // Para a música de fundo
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

    public void Dispose()
    {
        StopMusic();
        _eatSound?.Dispose();
        _deathSound?.Dispose();
        _eatGhostSound?.Dispose();
    }

    public void StopAll()
    {
        StopMusic();

        // 2. Para efeitos sonoros em andamento
        if (_isChomping && _eatSound != null)
        {
            _eatSound.Stop();
            _isChomping = false;
        }

        // Garante que a morte não fique tocando se o usuário clicar rápido
        _deathSound?.Stop();

        // Garante que o som do fantasma pare
        _eatGhostSound?.Stop();
    }

    public bool ToggleMusic()
    {
        _isMuted = !_isMuted;

        // Atualiza o volume da música que está tocando agora
        if (_bgmReader != null)
        {
            _bgmReader.Volume = _isMuted ? 0f : 0.15f;
        }

        return _isMuted; // Retorna o estado atual para atualizar a UI
    }
}
