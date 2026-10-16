using MediatR;

namespace InterviewPrep.Application.Purchases.Events;

/// <summary>
/// OBSERVER / PUB-SUB PATTERN via MediatR notifications: the command handler that creates a
/// Purchase doesn't know or care who's listening -- it just publishes this event. Today, one
/// in-process handler reacts (tier upgrade check). In a system with real scale (this is the honest
/// answer if asked "have you used Kafka/RabbitMQ"): you'd persist this to an Outbox table in the
/// SAME transaction as the Purchase insert, and a separate relay would publish it to Kafka/RabbitMQ
/// so other SERVICES (not just other classes in the same process) can react -- same pattern,
/// just crossing a process boundary instead of an in-memory one. HishabPOS already has the Outbox
/// building block (PurgeOutboxEventsJob) for exactly that future step.
/// </summary>
public sealed record PurchaseCreatedEvent(int MemberId, int PurchaseId, decimal FinalAmount) : INotification;
