﻿using Kenedia.Modules.Core.Services;

namespace Kenedia.Modules.Core.Models
{
    public class ServiceCollection
    {
        public ServiceCollection(GameState gameState, ClientWindowService clientWindowService, SharedSettings sharedSettings)
        {
            GameState = gameState;
            ClientWindowService = clientWindowService;
            SharedSettings = sharedSettings;
        }

        public GameState GameState { get; }

        public ClientWindowService ClientWindowService { get; }

        public SharedSettings SharedSettings { get; }
    }
}
