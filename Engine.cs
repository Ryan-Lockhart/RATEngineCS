using Raylib_cs;

using rat.Primitives;
using System;
using System.Numerics;
using rat.Constants;

namespace rat
{
    public class Engine
    {
        private int _FPS;

        private bool _fullscreen;
        private bool _locked;

        private GlyphSet _gameSet;
        private GlyphSet _uiSet;

        private Cursor _cursor;

        private Actor _player;
        private Map _map;

        private bool _playerActed;

        private bool _showLog;
        private bool _showControls;

        private bool _actionSelect;
        private Action _currentAction;
        private Coord _actionPosition;

        private List<Actor> _actors;
        private List<Actor> _living;
        private List<Actor> _dead;

        private DateTime _lastUpdateTime;
        private DateTime _lastSummonTime;

        private List<ConfigFlags> _currentFlags;

        public Engine(ulong seed)
        {
            Size windowSize = Screens.WindowSize * GlyphSets.DefaultMapGlyphs.glyphSize;

            Raylib.InitWindow((int)windowSize.width, (int)windowSize.height, "RATEngine");

            _currentFlags = new List<ConfigFlags>
            {
                ConfigFlags.FLAG_WINDOW_ALWAYS_RUN,
                ConfigFlags.FLAG_WINDOW_UNDECORATED,
                ConfigFlags.FLAG_WINDOW_TOPMOST,
                ConfigFlags.FLAG_VSYNC_HINT
            };

            var flags = _currentFlags
                .Cast<int>()
                .Aggregate(0, (s, f) => s | f);

            Raylib.SetWindowState((ConfigFlags)flags);
            Raylib.DisableCursor();

            _gameSet = new GlyphSet(GlyphSets.DefaultMapGlyphs);
            _uiSet = new GlyphSet(GlyphSets.DefaultUIGlyphs);

            bool exit = false;

            DateTime lastFrame = DateTime.Now;
            DateTime thisFrame = DateTime.Now;

            TimeSpan deltaTime;

            _map = new Map(mapSize, Bounds.Sixteen);

            _cursor = new Cursor(*ptr_Map, { 0, 0 }, { 1, 1 });

            _map->Generate(0.4f);
            _map->Smooth(5, 4);

            _map->Populate();

            ulong currentID = 0;

            _player = new Actor(currentID, "Jenkins", "A spry lad clad in armor and blade", Glyphs::ASCII::Player, 1, 10.0f, 5.0f, 7.5f, 0.50f, 0.75f, false, ptr_Map, false);
            currentID++;

            _actors = new List<Actor>();
            _living = new List<Actor>();
            _dead = new List<Actor>();

            _actors.Add(_player);
            _living.Add(_player);

            int totalEnemies = (size_t)Random::Generator->Next(minimumEnemies, maximumEnemies);

            SummonEnemies(ref currentID, totalEnemies);

            while (!exit)
            {
                lastFrame = thisFrame;
                thisFrame = DateTime.Now;
                deltaTime = thisFrame - lastFrame;

                //m_FPS = std::chrono::duration_cast<std::chrono::seconds>(deltaTime).count();
                _FPS = deltaTime.Milliseconds;

                SDL_Event e;

                KeyboardKey keyPressed = KeyboardKey.KEY_NULL;

                do
                {
                    keyPressed = (KeyboardKey)Raylib.GetKeyPressed();

                    switch (keyPressed)
                    {
                        case KeyboardKey.KEY_KP_1:

                            break;
                    }
                }
                while (keyPressed != KeyboardKey.KEY_NULL);
                

                while (SDL_PollEvent(&e) > 0)
                {
                    if (e.type == SDL_KEYDOWN && m_ActionSelect)
                    {
                        switch (e.key.keysym.sym)
                        {
                            case SDLK_1:
                                m_CurrentAction = Action::MoveTo;
                                m_ActionSelect = false;
                                break;
                            case SDLK_2:
                                m_CurrentAction = Action::LookAt;
                                m_ActionSelect = false;
                                break;
                            case SDLK_3:
                                m_CurrentAction = Action::Attack;
                                m_ActionSelect = false;
                                break;
                            case SDLK_4:
                                m_CurrentAction = Action::Push;
                                m_ActionSelect = false;
                                break;
                            case SDLK_5:
                                m_CurrentAction = Action::Mine;
                                m_ActionSelect = false;
                                break;
                        }
                    }

                    switch (e.type)
                    {
                        case SDL_KEYDOWN:
                            switch (e.key.keysym.sym)
                            {
                                case SDLK_ESCAPE:
                                    if (!m_ActionSelect)
                                        exit = true;
                                    else
                                    {
                                        m_CurrentAction = Action::None;
                                        m_ActionSelect = false;
                                    }
                                    break;
                                case SDLK_BACKSPACE:
                                    messageLog.clear();
                                    break;
                                case SDLK_TAB:
                                    if (!m_ActionSelect)
                                    {
                                        m_ActionSelect = true;
                                        m_CurrentAction = Action::None;
                                    }
                                    else m_ActionSelect = false;
                                    break;
                                default:
                                    Input(e.key.keysym.sym);
                                    break;
                            }
                    }
                }

                Update(deltaTime);
                Render(deltaTime);
            }
        }

        public void ToggleFullscreen() { m_Fullscreen = !m_Fullscreen; SDL_SetWindowFullscreen(ptr_Window, m_Fullscreen ? SDL_WINDOW_FULLSCREEN_DESKTOP : 0); }

        public virtual void Input(const SDL_Keycode& code);
		public virtual void Update(std::chrono::high_resolution_clock::duration deltaTime);
        public virtual void Render(std::chrono::high_resolution_clock::duration deltaTime);

        public void SetDrawColor(const Color& color);

		public void DrawRect(const Rect& transform, const Size& scale_by = { 1, 1 }, bool fill = false);
        public void DrawRect(const Point& position, const Size& size, const Size& scale_by = { 1, 1 }, bool fill = false);
        public void DrawRect(int x, int y, int width, int height, const Size& scale_by = { 1, 1 }, bool fill = false);

        public void DrawText(const std::string& text, const Point& position, const TextAlignment& alignment, const Color& color);
		public void DrawLabel(const std::string& text, const Point& position, const Size& padding, const TextAlignment& alignment, const Color& color);
		public void DrawFixedLabel(const std::string& text, const Rect& rect, const Size& padding, const TextAlignment& alignment, const Color& color);

		public void DrawCursor(const Cursor& cursor, bool attached = false);

        /// <summary>
        /// BRING OUT YER DEAD!
        /// </summary>
        public void CollectDead();

        /// <summary>
        /// Fetch their fetid souls from the warp!
        /// </summary>
        public void SummonEnemies(ref ulong currentID, int amount);
    }
}