using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SecretHitler.Application.LegacyEngine;
using SecretHitler.WebUI.Legacy;

namespace SecretHitler.WebUI.Controllers
{
    public class GameController : ApiControllerBase
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IMapper _mapper;

        public GameController(GameStateMachine stateMachine, GameDataAccessor gameRepo)
        {
            _stateMachine = stateMachine;
            _stateMachine.LoadGameState(gameRepo.GameData);
        }

        [HttpPost]
        [Route("")]
        public ActionResult Start()
        {
            _stateMachine.Start();
            return Ok();
        }

        [HttpGet]
        [Route("clients")]
        public ActionResult<ICollection<Guid>> GetClients()
        {
            return Ok(_stateMachine.GameData.Players.Select(_ => _.Identifier).ToList());
        }

        [HttpGet]
        [Route("{identifier:guid}")]
        public ActionResult<GameDataDto> Get(Guid identifier)
        {
            var result = _stateMachine.GenerateGameDataForPlayer(identifier);
            return result == null ? NotFound() : Ok(result);
        }
    }
}