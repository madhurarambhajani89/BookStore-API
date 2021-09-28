using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted GetAllAuthors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Successfully got all Authors");
                return Ok(response);
            }
            catch (Exception e)
            {
                InternalError($"{e.Message }-{e.InnerException}");
                throw;
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted GetAllAuthors:{id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"Author with {id} do not found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"Successfully got all Authors:{id}");
                return Ok(response);
            }
            catch (Exception e)
            {

                InternalError($"{e.Message }-{e.InnerException}");
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"Author Submission Attempted");
               
                if (authorDTO == null)
                {
                    _logger.LogWarn($"Empty Request was submitted");
                    return BadRequest (ModelState);
                }
                if(!ModelState.IsValid )
                {
                    _logger.LogWarn($"Author Data was incomplete");
                    return BadRequest(ModelState);

                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);
               if(!isSuccess)
                {
                    
                    return InternalError("Author creation failed");
                }
                _logger.LogInfo($"{location}: Creation was successful");
                return Created("Create", new { author });
                
            }
            catch (Exception e)
            {

                InternalError($"{e.Message }-{e.InnerException}");
                throw;
            }
        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id,[FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo("Author Updated Attempted -id:{id}");
                if(id<1|| authorDTO==null|| id!=authorDTO.Id)
                {
                    _logger.LogWarn($"Author Update failed with bad data");
                    return BadRequest();
                }
                var isExists = await _authorRepository.isExists(id);
                if(ModelState.IsValid )
                {
                    _logger.LogWarn($"Author data is incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                if(!isSuccess)
                {
                    return InternalError($"Update operation Operation");
                }
                return NoContent();
            }
            catch(Exception e)
            {

               return  InternalError($"{e.Message }-{e.InnerException}");
                throw;
            }
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                if (id < 1)
                {
                    return BadRequest();
                }

                var isExists = await _authorRepository.isExists(id);
                if (ModelState.IsValid)
                {
                    _logger.LogWarn($"Author data is incomplete");
                    return BadRequest(ModelState);
                }
                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (isSuccess)
                {
                    return InternalError($"Author deletefailed");
                }
                _logger.LogWarn($"Author with id:{id} successfully deleted");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message }-{e.InnerException}");
                throw;
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong please contact adminstrator");

        }
    }
};