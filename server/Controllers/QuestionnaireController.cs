using Microsoft.AspNetCore.Mvc;
using StadiumCompany.DAL;
using StadiumCompany.Models;

namespace StadiumCompany.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionnaireController : ControllerBase
{
    private readonly QuestionnaireRepository _questionnaireRepository = new();

    [HttpGet("user/{userId}")]
    public ActionResult<List<Questionnaire>> GetByUser(int userId)
    {
        try
        {
            var questionnaires = _questionnaireRepository.GetByUser(userId);
            return Ok(questionnaires);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("published/{currentUserId}")]
    public ActionResult<List<Questionnaire>> GetPublishedByOthers(int currentUserId)
    {
        try
        {
            var questionnaires = _questionnaireRepository.GetPublishedByOthers(currentUserId);
            return Ok(questionnaires);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    // Ajouter d'autres méthodes selon les besoins (Create, Update, Delete)
}