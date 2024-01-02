using AutoMapper;
using GloboTicket.TicketManagement.Application.Contracts.Infrastructure;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Exceptions;
using GloboTicket.TicketManagement.Application.Features.Categories.Commands.CreateCategory;
using GloboTicket.TicketManagement.Application.Models.Mail;
using GloboTicket.TicketManagement.Domain.Entities;
using MediatR;

namespace GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public CreateEventCommandHandler(IMapper mapper, IEmailService emailService, IEventRepository eventRepository)
    {
        _mapper = mapper;
        _emailService = emailService;
        _eventRepository = eventRepository;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = _mapper.Map<Event>(request);

        var validator = new CreateEventCommandValidator(_eventRepository);
        var validationResult = await validator.ValidateAsync(request);
        if(validationResult.Errors.Count > 0)
        {
            throw new ValidationException(validationResult);
        }

        @event = await _eventRepository.AddAsync(@event);

        var email = new Email() { 
            To = "admin@app.com",
            Body = $"A new event was created: {request}",
            Subject = "A new event was created"
        };

        try
        {
            await _emailService.SendEmail(email);
        }
        catch (Exception)
        {

            throw;
        }

        return @event.EventId;
    }
}