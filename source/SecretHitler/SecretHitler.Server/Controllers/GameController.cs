using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using SecretHitler.Game.Engine;

namespace SecretHitler.Server.Controllers
{
    [ApiController]
    [Route("{controller}")]
    public class GameController : ControllerBase
    {
        private readonly GameStateMachine _stateMachine;

        public GameController(GameStateMachine stateMachine, GameDataAccessor gameRepo)
        {
            _stateMachine = stateMachine;
            _stateMachine.LoadGameState(gameRepo.GameData);
        }

        [HttpPost]
        [Route("/")]
        public ActionResult Start()
        {
            _stateMachine.Start();
            return Ok();
        }
    }
}
