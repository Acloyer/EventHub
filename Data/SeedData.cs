using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventHub.Models;
using EventHub.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventHub.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, SeedDataDto? seedDataDto = null)
        {
            var context = serviceProvider.GetRequiredService<EventHubDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var logger = serviceProvider.GetRequiredService<ILogger<EventHubDbContext>>();

            // Use default values if not provided
            seedDataDto ??= new SeedDataDto();

            // Clear all existing data except Owner
            logger.LogInformation("Clearing existing data...");
            
            context.PostReactions.RemoveRange(context.PostReactions);
            context.EventComments.RemoveRange(context.EventComments);
            context.FavoriteEvents.RemoveRange(context.FavoriteEvents);
            context.PlannedEvents.RemoveRange(context.PlannedEvents);
            context.Events.RemoveRange(context.Events);
            
            // Remove all users except Owner
            var usersToRemove = context.Users.Where(u => u.Email != "owner@eventhub.com").ToList();
            context.Users.RemoveRange(usersToRemove);
            
            await context.SaveChangesAsync();
            logger.LogInformation("All existing data cleared.");

            // Create users
            logger.LogInformation("Creating users...");
            
            var users = new List<User>();
            var random = new Random();

            // Get existing Owner or create if not exists
            var existingOwner = await userManager.FindByEmailAsync("owner@eventhub.com");
            if (existingOwner != null)
            {
                users.Add(existingOwner);
                logger.LogInformation("Using existing Owner user");
            }
            else
            {
                // Create Owner if not exists
                var owner = new User 
                { 
                    UserName = "owner@eventhub.com", 
                    Email = "owner@eventhub.com", 
                    Name = "System Owner", 
                    EmailConfirmed = true 
                };
                users.Add(owner);
                logger.LogInformation("Creating new Owner user");
            }

            // Create Senior Admins
            for (int i = 0; i < seedDataDto.SeniorAdminCount; i++)
            {
                users.Add(new User 
                { 
                    UserName = $"senioradmin{i + 1}@eventhub.com", 
                    Email = $"senioradmin{i + 1}@eventhub.com", 
                    Name = $"Senior Administrator {i + 1}", 
                    EmailConfirmed = true 
                });
            }

            // Create Admins
            for (int i = 0; i < seedDataDto.AdminCount; i++)
            {
                users.Add(new User 
                { 
                    UserName = $"admin{i + 1}@eventhub.com", 
                    Email = $"admin{i + 1}@eventhub.com", 
                    Name = $"Admin {i + 1}", 
                    EmailConfirmed = true 
                });
            }

            // Create Organizers
            for (int i = 0; i < seedDataDto.OrganizerCount; i++)
            {
                users.Add(new User 
                { 
                    UserName = $"organizer{i + 1}@eventhub.com", 
                    Email = $"organizer{i + 1}@eventhub.com", 
                    Name = $"Event Organizer {i + 1}", 
                    EmailConfirmed = true 
                });
            }

            // Create Regular Users
            logger.LogInformation($"Creating {seedDataDto.RegularUserCount} regular users...");
            for (int i = 0; i < seedDataDto.RegularUserCount; i++)
            {
                var names = new[] { "John Smith", "Emma Johnson", "Michael Brown", "Sarah Davis", "David Wilson", 
                                   "Lisa Anderson", "Robert Taylor", "Jennifer Garcia", "Christopher Martinez", "Amanda Rodriguez",
                                   "James Wilson", "Maria Garcia", "William Johnson", "Elizabeth Brown", "Richard Davis",
                                   "Patricia Miller", "Joseph Wilson", "Linda Moore", "Thomas Taylor", "Barbara Anderson" };
                
                users.Add(new User 
                { 
                    UserName = $"user{i + 1}@eventhub.com", 
                    Email = $"user{i + 1}@eventhub.com", 
                    Name = names[i % names.Length], 
                    EmailConfirmed = true 
                });
            }
            logger.LogInformation($"Added {seedDataDto.RegularUserCount} regular users to the list. Total users: {users.Count}");

            // Create users with default password (skip Owner if already exists)
            foreach (var user in users)
            {
                if (user.Email == "owner@eventhub.com" && existingOwner != null)
                {
                    // Skip creating Owner if already exists
                    continue;
                }
                
                var result = await userManager.CreateAsync(user, "Password123!");
                if (!result.Succeeded)
                {
                    logger.LogError($"Failed to create user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Assign roles
            int userIndex = 0;
            
            // Assign Owner role (always only 1) - skip if already has role
            var ownerUser = users[userIndex++];
            var ownerRoles = await userManager.GetRolesAsync(ownerUser);
            if (!ownerRoles.Contains("Owner"))
            {
                await userManager.AddToRoleAsync(ownerUser, "Owner");
                logger.LogInformation("Owner role assigned to user");
            }
            else
            {
                logger.LogInformation("Owner user already has Owner role");
            }

            // Assign SeniorAdmin roles
            for (int i = 0; i < seedDataDto.SeniorAdminCount; i++)
            {
                await userManager.AddToRoleAsync(users[userIndex++], "SeniorAdmin");
            }

            // Assign Admin roles
            for (int i = 0; i < seedDataDto.AdminCount; i++)
            {
                await userManager.AddToRoleAsync(users[userIndex++], "Admin");
            }

            // Assign Organizer roles
            for (int i = 0; i < seedDataDto.OrganizerCount; i++)
            {
                await userManager.AddToRoleAsync(users[userIndex++], "Organizer");
            }

            // Assign 'User' role to all remaining users (Regular Users)
            logger.LogInformation($"Assigning 'User' role to {users.Count - userIndex} remaining users...");
            for (int i = userIndex; i < users.Count; i++)
            {
                await userManager.AddToRoleAsync(users[i], "User");
                logger.LogInformation($"Assigned 'User' role to {users[i].Email}");
            }
            logger.LogInformation($"Finished assigning roles. Total users with roles: {users.Count}");

            // Create events
            logger.LogInformation("Creating events...");
            
            var categories = new[] { "Technology", "Business", "Education", "Entertainment", "Sports", "Health", "Other" };
            var locations = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose" };
            
            var events = new List<Event>();
            var now = DateTime.UtcNow;

            // Get users who can create events (Organizer, Admin, SeniorAdmin, Owner)
            var eventCreators = new List<User>();
            foreach (var user in users)
            {
                var userRoles = await userManager.GetRolesAsync(user);
                if (userRoles.Any(r => r == "Organizer" || r == "Admin" || r == "SeniorAdmin" || r == "Owner"))
                {
                    eventCreators.Add(user);
                }
            }
            
            logger.LogInformation($"Found {eventCreators.Count} users who can create events");

            // Past events
            for (int i = 0; i < seedDataDto.PastEventCount; i++)
            {
                var startDate = now.AddDays(-random.Next(30, 365)); // 1-12 months ago
                var endDate = startDate.AddDays(random.Next(1, 7)); // 1-7 days duration
                
                events.Add(new Event
                {
                    Title = $"Past Event {i + 1}: {categories[random.Next(categories.Length)]} Conference",
                    Description = $"This was a great event about {categories[random.Next(categories.Length)].ToLower()} that took place in the past. It featured amazing speakers and networking opportunities.",
                    StartDate = startDate,
                    EndDate = endDate,
                    Category = categories[random.Next(categories.Length)],
                    Location = locations[random.Next(locations.Length)],
                    MaxParticipants = random.Next(50, 500),
                    CreatorId = eventCreators[random.Next(eventCreators.Count)].Id
                });
            }

            // Future events
            for (int i = 0; i < seedDataDto.FutureEventCount; i++)
            {
                var startDate = now.AddDays(random.Next(1, 365)); // 1-365 days in future
                var endDate = startDate.AddDays(random.Next(1, 7)); // 1-7 days duration
                
                events.Add(new Event
                {
                    Title = $"Future Event {i + 1}: {categories[random.Next(categories.Length)]} Summit",
                    Description = $"Join us for an exciting event about {categories[random.Next(categories.Length)].ToLower()}. This will be an amazing opportunity to learn and network with industry experts.",
                    StartDate = startDate,
                    EndDate = endDate,
                    Category = categories[random.Next(categories.Length)],
                    Location = locations[random.Next(locations.Length)],
                    MaxParticipants = random.Next(50, 500),
                    CreatorId = eventCreators[random.Next(eventCreators.Count)].Id
                });
            }

            context.Events.AddRange(events);
            await context.SaveChangesAsync();

            // Create comments
            logger.LogInformation("Creating comments...");
            
            var comments = new List<EventComment>();
            
            // Positive comments
            var positiveComments = new[]
            {
                "This looks like an amazing event! I'm definitely interested in attending.",
                "Great initiative! Looking forward to this event.",
                "The speakers lineup looks impressive. Can't wait!",
                "This is exactly what I was looking for. Count me in!",
                "The venue is perfect for this type of event.",
                "I've attended similar events before and they were fantastic.",
                "The timing works perfectly for me. See you there!",
                "This event will be very informative and useful.",
                "I'm excited about the networking opportunities.",
                "The agenda looks well-planned and comprehensive.",
                "This is a must-attend event for anyone in the field.",
                "I'm bringing my team to this event.",
                "The early bird pricing is very reasonable.",
                "I've already registered and can't wait!",
                "This event promises to be very educational.",
                "The location is convenient for most attendees.",
                "I'm sure this will be a great learning experience.",
                "The organizers have done a fantastic job with this event!",
                "This is exactly the kind of event our industry needs.",
                "Looking forward to the networking opportunities!"
            };

            // Neutral comments
            var neutralComments = new[]
            {
                "This will be my first time attending such an event.",
                "The topics covered are very relevant to my work.",
                "I'm looking forward to meeting other professionals.",
                "Interesting lineup of speakers.",
                "The event seems well-organized.",
                "I'm curious about the content.",
                "This could be a good learning opportunity.",
                "The timing works for my schedule.",
                "I'll consider attending this event.",
                "The venue location is accessible.",
                "The agenda looks comprehensive.",
                "I'm interested in the topics being covered.",
                "This event might be worth checking out.",
                "The format seems appropriate for the content.",
                "I'll wait to see the final details.",
                "The event description is informative.",
                "I'm considering bringing a colleague.",
                "The registration process seems straightforward.",
                "I'll keep this event in mind.",
                "The event structure looks reasonable."
            };

            // Negative comments
            var negativeComments = new[]
            {
                "The timing doesn't work for me at all.",
                "I'm not sure about the speaker lineup.",
                "The venue is too far from my location.",
                "The price seems a bit high for what's offered.",
                "I've been to similar events and they were disappointing.",
                "The agenda doesn't cover the topics I'm interested in.",
                "I'm concerned about the event organization.",
                "The format doesn't appeal to me.",
                "I don't think this event will be worth my time.",
                "The speakers don't seem very relevant to my field.",
                "I'm disappointed with the event structure.",
                "The location is inconvenient for most attendees.",
                "I expected more from this event.",
                "The content seems too basic for my level.",
                "I'm not impressed with the planning.",
                "The event seems overpriced.",
                "I don't see much value in attending.",
                "The speakers are not well-known in the industry.",
                "I'm skeptical about the event quality.",
                "This doesn't meet my expectations."
            };

            // Helper function to diversify comments
            string DiversifyComment(string comment, Random random)
            {
                var style = random.Next(10); // 10 different styles
                
                switch (style)
                {
                    case 0: // Add emoji at the end
                        var emojis = new[] { "😊", "👍", "🎉", "🔥", "👏", "🚀", "💯", "⭐", "🎯", "💪", "❤️", "😍", "🤩", "✨", "🌟" };
                        return comment + " " + emojis[random.Next(emojis.Length)];
                    
                    case 1: // Start with lowercase
                        return char.ToLower(comment[0]) + comment.Substring(1);
                    
                    case 2: // Add exclamation marks
                        return comment + "!!";
                    
                    case 3: // Add multiple exclamation marks
                        return comment + "!!!";
                    
                    case 4: // Add ellipsis
                        return comment + "...";
                    
                    case 5: // Add emoji at start and end
                        var startEmojis = new[] { "🎯", "💡", "📝", "🎪", "🎨", "🎭", "🎪", "🎯", "💡", "📝" };
                        var endEmojis = new[] { "😊", "👍", "🎉", "🔥", "👏", "🚀", "💯", "⭐", "🎯", "💪" };
                        return startEmojis[random.Next(startEmojis.Length)] + " " + comment + " " + endEmojis[random.Next(endEmojis.Length)];
                    
                    case 6: // Add "haha" or "lol"
                        var laughs = new[] { " haha", " lol", " 😂", " 😆" };
                        return comment + laughs[random.Next(laughs.Length)];
                    
                    case 7: // Add "btw" or "imo"
                        var prefixes = new[] { "btw ", "imo ", "tbh ", "fyi " };
                        return prefixes[random.Next(prefixes.Length)] + char.ToLower(comment[0]) + comment.Substring(1);
                    
                    case 8: // Add "..." at start
                        return "..." + comment;
                    
                    case 9: // Add "!" at start and end
                        return "!" + comment + "!";
                    
                    default:
                        return comment;
                }
            }

            // Create positive comments
            for (int i = 0; i < seedDataDto.PositiveCommentCount; i++)
            {
                var eventId = events[random.Next(events.Count)].Id;
                var userId = users[random.Next(users.Count)].Id;
                var commentText = positiveComments[random.Next(positiveComments.Length)];
                var diversifiedComment = DiversifyComment(commentText, random);
                
                comments.Add(new EventComment
                {
                    EventId = eventId,
                    UserId = userId,
                    Comment = diversifiedComment,
                    PostDate = now.AddDays(-random.Next(0, 30)), // Comments from last 30 days
                    IsEdited = false,
                    IsPinned = false
                });
            }

            // Create neutral comments
            for (int i = 0; i < seedDataDto.NeutralCommentCount; i++)
            {
                var eventId = events[random.Next(events.Count)].Id;
                var userId = users[random.Next(users.Count)].Id;
                var commentText = neutralComments[random.Next(neutralComments.Length)];
                var diversifiedComment = DiversifyComment(commentText, random);
                
                comments.Add(new EventComment
                {
                    EventId = eventId,
                    UserId = userId,
                    Comment = diversifiedComment,
                    PostDate = now.AddDays(-random.Next(0, 30)), // Comments from last 30 days
                    IsEdited = false,
                    IsPinned = false
                });
            }

            // Create negative comments
            for (int i = 0; i < seedDataDto.NegativeCommentCount; i++)
            {
                var eventId = events[random.Next(events.Count)].Id;
                var userId = users[random.Next(users.Count)].Id;
                var commentText = negativeComments[random.Next(negativeComments.Length)];
                var diversifiedComment = DiversifyComment(commentText, random);
                
                comments.Add(new EventComment
                {
                    EventId = eventId,
                    UserId = userId,
                    Comment = diversifiedComment,
                    PostDate = now.AddDays(-random.Next(0, 30)), // Comments from last 30 days
                    IsEdited = false,
                    IsPinned = false
                });
            }

            context.EventComments.AddRange(comments);
            await context.SaveChangesAsync();

            // Create reactions if enabled
            if (seedDataDto.CreateReactions)
            {
                logger.LogInformation("Creating reactions...");
                
                var reactions = new List<PostReaction>();
                var emojis = new[] { "👍", "❤️", "🎉", "🔥", "👏", "🚀", "💯", "⭐", "🎯", "💪" };

                foreach (var user in users)
                {
                    var randomEvent = events[random.Next(events.Count)];
                    var randomEmoji = emojis[random.Next(emojis.Length)];
                    
                    reactions.Add(new PostReaction
                    {
                        EventId = randomEvent.Id,
                        UserId = user.Id,
                        Emoji = randomEmoji
                    });
                }

                context.PostReactions.AddRange(reactions);
                await context.SaveChangesAsync();
            }

            // Create favorites if enabled
            if (seedDataDto.CreateFavorites)
            {
                logger.LogInformation("Creating favorites...");
                
                var favorites = new List<FavoriteEvent>();

                foreach (var user in users)
                {
                    var randomEvent = events[random.Next(events.Count)];
                    
                    favorites.Add(new FavoriteEvent
                    {
                        UserId = user.Id,
                        EventId = randomEvent.Id
                    });
                }

                context.FavoriteEvents.AddRange(favorites);
                await context.SaveChangesAsync();
            }

            // Create planned events if enabled
            if (seedDataDto.CreatePlannedEvents)
            {
                logger.LogInformation("Creating planned events...");
                
                var plannedEvents = new List<PlannedEvent>();

                foreach (var user in users)
                {
                    var randomEvent = events[random.Next(events.Count)];
                    
                    plannedEvents.Add(new PlannedEvent
                    {
                        UserId = user.Id,
                        EventId = randomEvent.Id
                    });
                }

                context.PlannedEvents.AddRange(plannedEvents);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Database seeding completed successfully!");
        }
    }
} 