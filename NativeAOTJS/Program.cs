using Jint;
using Jint.Runtime.Interop;
using NativeAOTJS;

public record Contributor(
    string UserName,
    int CommitCount,
    int LinesAdded,
    int LinesRemoved,
    DateTime FirstContributionDate,
    DateTime LastContributionDate)
{
    public DateTime LastContributionDate { get; set; } = LastContributionDate;
}
public class VerifiedPublisher
{
    public string UserName { get; set; }           // GitHub username or ID
    public double Score { get; set; }              // Trust score
    public Contributor? Contributor { get; set; }  // Optional, may be null
    public string Reason { get; set; }             // Optional: e.g. "Top N", "Threshold", "Manual Trust"
    public List<PluginListing> PluginListings { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public List<Report> Reports { get; set; } = new();
}

public class Review
{
    public string Reviewer { get; set; } = null!;
    public DateTime Date { get; set; }
    public int Stars { get; set; }  // 1 to 5 stars
    public string? Comment { get; set; }

    public bool IsPositive => Stars >= 4;  // 4 or 5 stars is positive
}
public class PluginListing
{
    public string Id { get; set; } = null!;          // Unique plugin identifier
    public string Name { get; set; } = null!;        // Plugin display name
    public bool IsPublic { get; set; }                         // Public or private listing
    public DateTime UploadDate { get; set; }                   // When it was uploaded
    public List<Review> Reviews { get; set; } = [];         // Reviews on this specific plugin
    public int Downloads { get; set; }                          // Optional: number of downloads
}

public enum ReportOutcome
{
    Pending,
    Valid,
    Invalid,
    Rejected,
}


public class Report
{
    public string Reporter { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Details { get; set; } = null!;
    public ReportOutcome Outcome { get; set; } = ReportOutcome.Pending;
    public string? ModeratorComment { get; set; }

    // A report is considered successful if it led to punishment or warning
    public bool Resolved => Outcome == ReportOutcome.Valid;
}

public class VerifiedPublisherSelector
{
    private const int GuaranteedCount = 5;
    private const double ScoreThreshold = 450.0; // absolute minimum floor
    private const int MinCommitsForAbuseCheck = 50;      // minimum commits to trigger abuse check
    private const double MinLocPerCommit = 20.0;         // minimum average lines changed per commit (added + removed)
    private const double AbusePenaltyMin = 0.2;          // minimum multiplier (80% penalty)
    private const double AbusePenaltyMax = 1.0;          // max multiplier (no penalty)
    private static readonly List<VerifiedPublisher> ManuallyTrustedAccounts =
    [
        new()
        {
            UserName = "some-org-owner",
            Score = 0, // initial score, will be calculated later
            Reason = "Manually trusted",
            PluginListings =
            [
                new() { Name = "SuperPlugin", IsPublic = true },
                new() { Name = "HelperTool", IsPublic = true }
            ],
            Reviews =
            [
                new() { Reviewer = "user1", Stars = 5, Comment = "Works flawlessly!" },
                new() { Reviewer = "user2", Stars = 4, Comment = "Very useful plugin." }
            ],
            Reports =
            [
                new Report { Reporter = "mod1", Outcome = ReportOutcome.Valid, Details = "Malicious code removed" },
                new Report { Reporter = "mod2", Outcome = ReportOutcome.Invalid, Details = "False alarm" }
            ]
        },

        new()
        {
            UserName = "cooldev123",
            Score = 0,
            Reason = "Manually trusted",
            PluginListings = [new PluginListing { Name = "AutoUpdater", IsPublic = true }],
            Reviews =
            [
                 new() { Reviewer = "user3", Stars = 5, Comment = "Saved me so much time!" },
                 new() { Reviewer = "user4", Stars = 3, Comment = "Decent but needs more features." }
            ],
            Reports =
            [
                new()
                {
                    Reporter = "mod3", Outcome = ReportOutcome.Invalid, Details = "Security vulnerability fixed"
                }
            ]
        },

        new()
        {
            UserName = "ElitePublisherPro",
            Score = 0, // This will be calculated by the algorithm
            Reason = "High volume, high quality, and trustworthy contributions",
            PluginListings = new()
            {
                new() { Name = "Plugin A", IsPublic = true, Downloads = 10000 },
                new() { Name = "Plugin B", IsPublic = true, Downloads = 10000 },
                new() { Name = "Plugin C", IsPublic = true, Downloads = 10000 },
                new() { Name = "Plugin D", IsPublic = true, Downloads = 10000 },
                new() { Name = "Plugin E", IsPublic = true, Downloads = 10000 },
                new() { Name = "Plugin F", IsPublic = true, Downloads = 10000 },
                new() { Name = "Plugin G", IsPublic = true, Downloads = 10000 },
                new() { Name = "Plugin H", IsPublic = true, Downloads = 10000 },
                new() { Name = "Plugin I", IsPublic = true },
                new() { Name = "Plugin J", IsPublic = true }
            },
            Reviews = new List<Review>
            {
                // 90 positive reviews (implied by Stars >= 4)
                 new() { Reviewer = "user1", Stars = 5, Comment = "Fantastic plugins, always reliable!" },
                 new() { Reviewer = "user2", Stars = 5, Comment = "ElitePublisherPro delivers top-notch quality." },
                 new() { Reviewer = "user5", Stars = 4, Comment = "Great work, highly recommend." },
                 new() { Reviewer = "user6", Stars = 5, Comment = "My go-to for essential tools." },
                 new() { Reviewer = "user7", Stars = 5, Comment = "Always impressed with their updates." },
                 new() { Reviewer = "user8", Stars = 4, Comment = "Solid and dependable." },
                 new() { Reviewer = "user9", Stars = 5, Comment = "Excellent support and features." },
                 new() { Reviewer = "user10", Stars = 5, Comment = "Highly productive tools." },
                 new() { Reviewer = "user11", Stars = 4, Comment = "Very satisfied with their offerings." },
                 new() { Reviewer = "user12", Stars = 5, Comment = "A true asset to the community." },
                 new() { Reviewer = "user15", Stars = 5, Comment = "Consistently high quality." },
                 new() { Reviewer = "user16", Stars = 4, Comment = "Reliable and well-maintained." },
                 new() { Reviewer = "user17", Stars = 5, Comment = "Indispensable tools." },
                 new() { Reviewer = "user18", Stars = 5, Comment = "Never disappoints." },
                 new() { Reviewer = "user19", Stars = 4, Comment = "Always improving." },
                 new() { Reviewer = "user20", Stars = 5, Comment = "A pleasure to use." },
                 new() { Reviewer = "user21", Stars = 5, Comment = "Top-tier development." },
                 new() { Reviewer = "user22", Stars = 4, Comment = "Excellent value." },
                 new() { Reviewer = "user23", Stars = 5, Comment = "Highly recommended by peers." },
                 new() { Reviewer = "user24", Stars = 5, Comment = "Seamless integration." },
                 new() { Reviewer = "user25", Stars = 5, Comment = "Fantastic performance." },
                 new() { Reviewer = "user26", Stars = 4, Comment = "Very robust and stable." },
                 new() { Reviewer = "user27", Stars = 5, Comment = "Always my first choice." },
                 new() { Reviewer = "user28", Stars = 5, Comment = "Simply the best." },
                 new() { Reviewer = "user29", Stars = 4, Comment = "Great features and easy to use." },
                 new() { Reviewer = "user30", Stars = 5, Comment = "Revolutionized my workflow." },
                 new() { Reviewer = "user31", Stars = 5, Comment = "Exceptional quality." },
                 new() { Reviewer = "user32", Stars = 4, Comment = "Very happy with this publisher." },
                 new() { Reviewer = "user33", Stars = 5, Comment = "Consistently delivers." },
                 new() { Reviewer = "user34", Stars = 5, Comment = "A true professional." },
                 new() { Reviewer = "user35", Stars = 4, Comment = "Reliable and efficient." },
                 new() { Reviewer = "user36", Stars = 5, Comment = "Highly effective tools." },
                 new() { Reviewer = "user37", Stars = 5, Comment = "Always a pleasure to use their plugins." },
                 new() { Reviewer = "user38", Stars = 4, Comment = "Top-notch security." },
                 new() { Reviewer = "user39", Stars = 5, Comment = "Great performance boost." },
                 new() { Reviewer = "user40", Stars = 5, Comment = "Essential for any serious project." },
                 new() { Reviewer = "user41", Stars = 5, Comment = "Fantastic user experience." },
                 new() { Reviewer = "user42", Stars = 4, Comment = "Very intuitive." },
                 new() { Reviewer = "user43", Stars = 5, Comment = "Highly versatile." },
                 new() { Reviewer = "user44", Stars = 5, Comment = "Always up-to-date." },
                 new() { Reviewer = "user45", Stars = 4, Comment = "Excellent documentation." },
                 new() { Reviewer = "user46", Stars = 5, Comment = "A must-have." },
                 new() { Reviewer = "user47", Stars = 5, Comment = "Brilliant solutions." },
                 new() { Reviewer = "user48", Stars = 4, Comment = "Very helpful." },
                 new() { Reviewer = "user49", Stars = 5, Comment = "Couldn't work without them." },
                 new() { Reviewer = "user50", Stars = 5, Comment = "Simply outstanding." },
                 new() { Reviewer = "user51", Stars = 5, Comment = "Highly effective." },
                 new() { Reviewer = "user52", Stars = 4, Comment = "Great support." },
                 new() { Reviewer = "user53", Stars = 5, Comment = "Very reliable." },
                 new() { Reviewer = "user54", Stars = 5, Comment = "Top choice." },
                 new() { Reviewer = "user55", Stars = 4, Comment = "Well-designed." },
                 new() { Reviewer = "user56", Stars = 5, Comment = "Highly functional." },
                 new() { Reviewer = "user57", Stars = 5, Comment = "Excellent value." },
                 new() { Reviewer = "user58", Stars = 4, Comment = "Very useful." },
                 new() { Reviewer = "user59", Stars = 5, Comment = "Highly recommended." },
                 new() { Reviewer = "user60", Stars = 5, Comment = "A true lifesaver." },
                 new() { Reviewer = "user61", Stars = 5, Comment = "Exceptional." },
                 new() { Reviewer = "user62", Stars = 4, Comment = "Very good." },
                 new() { Reviewer = "user63", Stars = 5, Comment = "Perfect." },
                 new() { Reviewer = "user64", Stars = 5, Comment = "Amazing." },
                 new() { Reviewer = "user65", Stars = 4, Comment = "Solid." },
                 new() { Reviewer = "user66", Stars = 5, Comment = "Brilliant." },
                 new() { Reviewer = "user67", Stars = 5, Comment = "Fantastic." },
                 new() { Reviewer = "user68", Stars = 4, Comment = "Great." },
                 new() { Reviewer = "user69", Stars = 5, Comment = "Superb." },
                 new() { Reviewer = "user70", Stars = 5, Comment = "Awesome." },
                 new() { Reviewer = "user71", Stars = 5, Comment = "Excellent." },
                 new() { Reviewer = "user72", Stars = 4, Comment = "Useful." },
                 new() { Reviewer = "user73", Stars = 5, Comment = "Reliable." },
                 new() { Reviewer = "user74", Stars = 5, Comment = "Effective." },
                 new() { Reviewer = "user75", Stars = 4, Comment = "Good." },
                 new() { Reviewer = "user76", Stars = 5, Comment = "Top." },
                 new() { Reviewer = "user77", Stars = 5, Comment = "Best." },
                 new() { Reviewer = "user78", Stars = 4, Comment = "Nice." },
                 new() { Reviewer = "user79", Stars = 5, Comment = "Cool." },
                 new() { Reviewer = "user80", Stars = 5, Comment = "Strong." },
                 new() { Reviewer = "user81", Stars = 5, Comment = "Powerful." },
                 new() { Reviewer = "user82", Stars = 4, Comment = "Clean." },
                 new() { Reviewer = "user83", Stars = 5, Comment = "Efficient." },
                 new() { Reviewer = "user84", Stars = 5, Comment = "Fast." },
                 new() { Reviewer = "user85", Stars = 4, Comment = "Smooth." },
                 new() { Reviewer = "user86", Stars = 5, Comment = "Stable." },
                 new() { Reviewer = "user87", Stars = 5, Comment = "Secure." },
                 new() { Reviewer = "user88", Stars = 4, Comment = "Modern." },
                 new() { Reviewer = "user89", Stars = 5, Comment = "Innovative." },
                 new() { Reviewer = "user90", Stars = 5, Comment = "Cutting-edge." },
                // 10 occasional bad reviews (implied by Stars < 4)
                new Review
                {
                    Reviewer = "user91", Stars = 2, Comment = "Had a minor bug once, but it was fixed quickly."
                },
                 new() { Reviewer = "user92", Stars = 3, Comment = "Some plugins could use more features." },
                 new() { Reviewer = "user93", Stars = 1, Comment = "Encountered a small compatibility issue." },
                 new() { Reviewer = "user94", Stars = 2, Comment = "Support was a bit slow once." },
                new Review
                {
                    Reviewer = "user95", Stars = 3, Comment = "Documentation could be clearer on one plugin."
                },
                 new() { Reviewer = "user96", Stars = 2, Comment = "Minor performance dip on a specific setup." },
                 new() { Reviewer = "user97", Stars = 1, Comment = "Ran into an unexpected error once." },
                 new() { Reviewer = "user98", Stars = 2, Comment = "UI could be more intuitive for one tool." },
                 new() { Reviewer = "user99", Stars = 3, Comment = "Feature request not yet implemented." },
                 new() { Reviewer = "user100", Stars = 2, Comment = "A small issue with a recent update." }
            },
            Reports = new()
            {
                // 20 successful reports (Outcome.Valid)
                new()
                {
                    Reporter = "mod1", Outcome = ReportOutcome.Valid, Details = "Resolved minor compatibility issue."
                },
                new()
                {
                    Reporter = "mod2", Outcome = ReportOutcome.Valid,
                    Details = "Addressed reported performance bottleneck."
                },
                new Report { Reporter = "mod4", Outcome = ReportOutcome.Valid, Details = "Fixed reported UI glitch." },
                new()
                {
                    Reporter = "mod5", Outcome = ReportOutcome.Valid,
                    Details = "Security patch implemented as reported."
                },
                new()
                {
                    Reporter = "mod6", Outcome = ReportOutcome.Valid, Details = "Corrected data integrity error."
                },
                new()
                {
                    Reporter = "mod7", Outcome = ReportOutcome.Valid, Details = "Improved error handling as suggested."
                },
                new Report { Reporter = "mod8", Outcome = ReportOutcome.Valid, Details = "Optimized resource usage." },
                new()
                {
                    Reporter = "mod9", Outcome = ReportOutcome.Valid, Details = "Fixed reported crash on specific OS."
                },
                new()
                {
                    Reporter = "mod10", Outcome = ReportOutcome.Valid, Details = "Resolved localization issue."
                },
                new()
                {
                    Reporter = "mod11", Outcome = ReportOutcome.Valid, Details = "Improved documentation clarity."
                },
                new()
                {
                    Reporter = "mod12", Outcome = ReportOutcome.Valid, Details = "Addressed accessibility concerns."
                },
                new()
                {
                    Reporter = "mod13", Outcome = ReportOutcome.Valid, Details = "Fixed reported memory leak."
                },
                new()
                {
                    Reporter = "mod14", Outcome = ReportOutcome.Valid,
                    Details = "Resolved conflict with other popular plugin."
                },
                new()
                {
                    Reporter = "mod15", Outcome = ReportOutcome.Valid,
                    Details = "Implemented requested feature enhancement."
                },
                new()
                {
                    Reporter = "mod16", Outcome = ReportOutcome.Valid, Details = "Corrected reported miscalculation."
                },
                new()
                {
                    Reporter = "mod17", Outcome = ReportOutcome.Valid, Details = "Improved network stability."
                },
                new()
                {
                    Reporter = "mod18", Outcome = ReportOutcome.Valid, Details = "Fixed reported installation issue."
                },
                new()
                {
                    Reporter = "mod19", Outcome = ReportOutcome.Valid, Details = "Addressed reported UI responsiveness."
                },
                new()
                {
                    Reporter = "mod20", Outcome = ReportOutcome.Valid, Details = "Resolved reported permission error."
                },
                new()
                {
                    Reporter = "mod21", Outcome = ReportOutcome.Valid, Details = "Fixed reported display corruption."
                }
            }
        }
    ];

    public static List<VerifiedPublisher> DetermineVerifiedPublishers(List<Contributor> contributors)
    {
        var scored = contributors
            .Select(c => new
            {
                Contributor = c,
                Score = CalculateCodeContributionScore(c)
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var verified = new List<VerifiedPublisher>();

        // Top N contributors
        foreach (var entry in scored.Take(GuaranteedCount))
        {
            verified.Add(new VerifiedPublisher
            {
                UserName = entry.Contributor.UserName,
                Score = entry.Score,
                Contributor = entry.Contributor,
                Reason = "Top 5 ShareX Contrib"
            });
        }

        // Contributors above threshold
        foreach (var entry in scored.Skip(GuaranteedCount))
        {
            if (entry.Score >= ScoreThreshold)
            {
                verified.Add(new VerifiedPublisher
                {
                    UserName = entry.Contributor.UserName,
                    Score = entry.Score,
                    Contributor = entry.Contributor,
                    Reason = "ShareX contrib"
                });
            }
            else
            {
                Console.WriteLine($"Contributor {entry.Contributor.UserName} did NOT make the cut. Their score was {entry.Score}");
            }
        }

        // Additional trusted accounts (e.g., orgs, hardcoded)
        verified.AddRange(
            ManuallyTrustedAccounts.Where(trusted => verified.All(v => v.UserName != trusted.UserName))
        );

        foreach (var vp in verified)
        {
            double trustScoreBonus = CalculatePlatformTrustScore(vp);

            vp.Score += trustScoreBonus;
        }
        return verified;
    }


    public static double CalculateCodeContributionScore(Contributor c)
    {
        if (c.UserName.Contains("[bot]"))
        {
            // AND STAY OUT!!!
            return -1;
        }
        if (c.LastContributionDate == DateTime.MinValue) c.LastContributionDate = c.FirstContributionDate;
        var monthsSinceLastContribution = (DateTime.UtcNow - c.LastContributionDate).TotalDays / 30.0;

        double recencyFactor = monthsSinceLastContribution switch
        {
            <= 1 => 1.55,
            <= 3 => 1.5,
            <= 6 => 1.3,
            <= 12 => 1.1,
            <= 24 => 0.6, 
            // Moderate decay for activity between 1 and 2 years
            // For activity older than 2 years, apply continuous decay based on full years inactive.
            // This creates the "score rot" effect for every additional year.
            _ => Math.Max(0.4 * Math.Pow(0.5, (int)Math.Floor((monthsSinceLastContribution - 24) / 12.0)), 0.56)
        };
        // Punish drive by contributors GREATLY
        if (c.LastContributionDate == c.FirstContributionDate && recencyFactor > 1.4)
        {
            recencyFactor *= 0.75; // apply 25% penalty to recency factor
        }

        var activeMonths = (c.LastContributionDate - c.FirstContributionDate).TotalDays / 30.0;
        double longevityFactor = Math.Log((activeMonths / 12.0) + 1, 2) * 0.25;
        longevityFactor = Math.Min(longevityFactor, 1.0); // Cap the *additional* longevity impact

        int netLines = Math.Max(0, c.LinesAdded - c.LinesRemoved);

        // Reduced commit count weight, increased net lines weight
        double baseScore =
            c.CommitCount * 0.3 +
            netLines * 0.02 +                 // Net value — positive if they wrote more than deleted
            c.LinesAdded * 0.05 +            // Favor actual creation
            c.LinesRemoved * 0.025;          // Still give some credit for clean-up
        double score = baseScore * recencyFactor * (0.7 + 0.3 * longevityFactor);
        // Anti-abuse: penalize contributors with many commits but very low code changes per commit
        if (c.CommitCount >= MinCommitsForAbuseCheck)
        {
            double locChanged = c.LinesAdded + c.LinesRemoved;
            double locPerCommit = locChanged / c.CommitCount;


            // Calculate a penalty factor that reduces score based on how little code they changed per commit
            // The penalty scales smoothly:
            // - If they average 20 or more lines changed per commit, no penalty (penalty factor = 1)
            // - If they average 5 or fewer lines per commit, max penalty (penalty factor = 0.2)
            // - Between 5 and 20 lines per commit, penalty scales linearly between 0.2 and 1.0
            double maxLocPerCommit = 50.0;  // Above this gets no penalty
            double clampedLocPerCommit = Math.Clamp(locPerCommit, MinLocPerCommit, maxLocPerCommit);

// Linear interpolation
            double penaltyFactor = AbusePenaltyMin +
                                   (clampedLocPerCommit - MinLocPerCommit) / (maxLocPerCommit - MinLocPerCommit) *
                                   (AbusePenaltyMax - AbusePenaltyMin);
            
            score *= penaltyFactor;
            if (penaltyFactor < 1.0)
            {
                // Log potential abuser for review or audit purposes
                Console.WriteLine($"[Abuse Detection] Contributor '{c.UserName}' flagged: " +
                                  $"CommitCount={c.CommitCount}, AvgLocPerCommit={locPerCommit:F2}, PenaltyFactor={penaltyFactor:F2}, Score={score:F2}");
            }
        }
        double cap = 1000;
        if (score <= cap)
            return score;


        // Use logarithmic scale for overflow to normalize values and add more of a ceiling.
        // The previous issue was that even with a multiplier like '150', the initial
        // increase for scores just above the cap (like L1Q's) could still be quite significant,
        // leading to a perception of the score "doubling" from its near-cap value.
        // This is because the logarithmic function has its steepest slope immediately after its origin.

        // Previous uncapped leaderboard
        // Jaex - Score: 366150.00 (Top 5 ShareX Contrib)
        // BrycensRanch - Score: 42104.74 (Top 5 SnapX Contrib)
        // McoreD - Score: 1446.35 (Top 5 ShareX Contrib)
        // sylveon - Score: 1401.83 (Top 5 ShareX Contrib)
        // L1Q - Score: 1175.72 (Top 5 ShareX Contrib)
        
        // After:
        // Jaex - Score: 9032.44 (Top 5 ShareX Contrib)
        // BrycensRanch - Score: 5924.12 (Top 5 SnapX Contrib)
        // McoreD - Score: 1399.25 (Top 5 ShareX Contrib)
        // sylveon - Score: 1364.04 (Top 5 ShareX Contrib)
        // L1Q - Score: 1170.59 (Top 5 ShareX Contrib)
        
        // To address this, we introduce a 'dampingFactor'.
        // This factor effectively
        // "flattens" the initial part of the logarithmic curve, making the increase
        // much more gradual for scores that are just slightly above the cap.
        // A larger dampingFactor means a flatter initial curve.
        double dampingFactor = 1400; // Adjust this value (e.g., 5, 20) to fine-tune the initial flatness.

        // The 'multiplier' then scales the output of the dampened logarithm to provide
        // sufficient differentiation for very high scores (like Jaex vs. BrycensRanch).
        // This value will likely need to be higher than previous multipliers due to the damping.
        double multiplier = 1000; // Adjust this to control the overall spread of high scores.

        // Calculate the overflow amount (how much the score exceeds the cap).
        double overflow = score - cap;

        // Apply the dampened logarithmic scale.
        // We add 1 to the result of (overflow / dampingFactor)
        // to ensure Math.Log does not receive 0 or negative values, and to make log(1) = 0
        // for the first point exactly at the cap (if overflow is 0).
        return cap + Math.Log((overflow / dampingFactor) + 1, 2) * multiplier;
    }
    public static double CalculatePlatformTrustScore(VerifiedPublisher vp)
{
    // Define weights for different factors.
    // These are significantly increased
    // to produce larger scores that can compete with the code contribution leaderboard.
    // Emphasis is placed on downloads and positive reviews.
    const double pluginUploadWeight = 500;
    const double pluginDownloadWeight = 450; // High emphasis on total plugin downloads
    const double pluginReviewWeight = 50;    // Strong emphasis on positive reviews for plugins

    const double positiveReviewWeight = 350;  // Very high emphasis on overall positive reviews
    const double negativeReviewPenalty = 300; // Significant penalty for negative reviews
    const double successfulReportWeight = 350; // Reports are "icing on top" - still contribute,
                                               // but less than primary factors
    const double failedReportPenalty = 450;    // Penalty for failed reports, also less dominant

    // Helper function to apply logarithmic scaling to counts.
    // This compresses large numbers, so the weights become crucial for final score magnitude.
    double ScaleTrustFactor(int count) => Math.Log(count + 1);

    // Determine if a review is positive based on star rating.
    // Assuming 4 or 5 stars are positive, and less than 4 are negative.
    bool IsPositiveReview(Review r) => r.IsPositive;

    // Calculate overall positive and negative reviews for the publisher.
    int positiveReviews = vp.Reviews.Count(IsPositiveReview);
    int negativeReviews = vp.Reviews.Count(r => !IsPositiveReview(r));

    // Calculate successful and failed reports for the publisher.
    int successfulReports = vp.Reports.Count(r => r.Outcome == ReportOutcome.Valid);
    int failedReports = vp.Reports.Count(r => r.Outcome == ReportOutcome.Invalid || r.Outcome == ReportOutcome.Rejected);

    // Aggregate plugin-based factors from all listed plugins.
    // NOTE: This assumes PluginListing objects have 'Downloads' and 'Reviews' properties,
    // even if they were not explicitly initialized in the example data structure.
    int pluginCount = vp.PluginListings.Count;
    int totalPluginDownloads = vp.PluginListings.Sum(p => p.Downloads); // Sum downloads across all plugins
    int totalPluginPositiveReviews = vp.PluginListings.Sum(p => p.Reviews.Count(IsPositiveReview)); // Sum positive reviews across all plugins
    int totalPluginNegativeReviews = vp.PluginListings.Sum(p => p.Reviews.Count(r => !IsPositiveReview(r))); // Sum negative reviews across all plugins

    // Calculate scores for each factor using the defined weights and logarithmic scaling.
    double pluginScore = ScaleTrustFactor(pluginCount) * pluginUploadWeight;
    double downloadScore = ScaleTrustFactor(totalPluginDownloads) * pluginDownloadWeight;
    double pluginReviewScore = ScaleTrustFactor(totalPluginPositiveReviews) * pluginReviewWeight;
    double pluginNegativeReviewPenalty = ScaleTrustFactor(totalPluginNegativeReviews) * negativeReviewPenalty;

    double positiveReviewScore = ScaleTrustFactor(positiveReviews) * positiveReviewWeight;
    double negativeReviewScore = ScaleTrustFactor(negativeReviews) * negativeReviewPenalty;
    double successfulReportScore = ScaleTrustFactor(successfulReports) * successfulReportWeight;
    double failedReportScore = ScaleTrustFactor(failedReports) * failedReportPenalty;

    // Combine all scores to get the final trust score.
    double trustScore = pluginScore
                        + downloadScore
                        + pluginReviewScore
                        - pluginNegativeReviewPenalty
                        + positiveReviewScore
                        - negativeReviewScore
                        + successfulReportScore
                        - failedReportScore;

    // Ensure the trust score does not go below zero.
    return Math.Max(0, trustScore);
}

}

class Program
{
    static void Main()
    {
        var source = new MyEventSource();

var engine = new Engine(options =>
{
    options.Strict();                      // Enable strict mode, improves performance
    // options.LimitRecursion(100);          // Limit recursion depth
    // options.TimeoutInterval(TimeSpan.FromSeconds(1)); // Execution timeout
    // options.MaxStatements(1_000);         // Limit number of executed statements
    options.AllowClr(typeof(Console).Assembly, typeof(Program).Assembly, typeof(MyEventSource).Assembly, typeof(Newtonsoft.Json.JsonConvert).Assembly); // Enable access to .NET types if needed

    options.ExperimentalFeatures = ExperimentalFeature.All;
}).SetValue("source", source);
engine = engine.SetValue("Newtonsoft", TypeReference.CreateTypeReference<Newtonsoft.Json.JsonConverter>(engine));

engine.Execute(@"
var log = System.Console.WriteLine;
            // Template literals
            const name = 'Jint';
            log(`Hello from ${name}`);

            // Arrow functions
            const double = x => x * 2;
            log(`Double 4 = ${double(4)}`);

            // Destructuring
            const [x, y] = [10, 20];
            const { a, b } = { a: 1, b: 2 };
            log(`Destructured: x=${x}, y=${y}, a=${a}, b=${b}`);

            // Class & inheritance
            class Animal {
                constructor(name) {
                    this.name = name;
                }
                speak() {
                    return `${this.name} makes a noise.`;
                }
            }

            class Dog extends Animal {
                speak() {
                    return `${this.name} barks.`;
                }
            }

            const d = new Dog('Rex');
            log(d.speak());

            // Rest/spread
            function sum(...nums) {
                return nums.reduce((acc, n) => acc + n, 0);
            }
            log(`Sum = ${sum(1, 2, 3, 4)}`);

            // Map / Set
            const map = new Map();
            map.set('foo', 123);
            log(`Map foo = ${map.get('foo')}`);

            const set = new Set([1, 2, 2, 3]);
            log(`Set size = ${set.size}`);

            // Symbol
            const sym2 = Symbol('id');
            log(`Symbol: ${sym2.toString()}`);

            // Promise (sync example)
            new Promise(resolve => resolve('Resolved'))
                .then(val => log(`Promise result: ${val}`));
        ");

engine.Execute(@"
var log = System.Console.WriteLine;
            function* counter() {
                let i = 0;
                while (i < 4) {
                    log('yielding: ' + i);
                    yield i++;
                }
            }

            log('--- gen1 (for..of) ---');
            const gen1 = counter();
            for (const value of gen1) {
                log('Generator yield: ' + value);
            }

            log('--- gen2 (manual) ---');
const gen2 = counter();
let result;

result = gen2.next();
log('Next: ' + result.value);

result = gen2.next();
log('Next: ' + result.value);

result = gen2.next();
log('Next: ' + result.value);

result = gen2.next();
log('Done? ' + result.done);
var file = new System.IO.StreamWriter('log.txt');
file.WriteLine('Hello World !');
            // Create a delegate matching EventHandler<string> signature
            const handler = function(sender, message) {
                log('JS received message: ' + message);
source.remove_Message(handler);

            };

            // Subscribe to event
            source.add_Message(handler);
        ");
source.Raise("Hello from .NET");
engine.Execute("""
var Newtonsoft = importNamespace('Newtonsoft.Json');
var json = '{\"name\":\"romvnly\",\"age\":30}';
var obj = Newtonsoft.JsonConvert.DeserializeObject(json);
log('Name: ' + obj.name);
log('Age: ' + obj.age);
"""
);
engine.Execute("""
               log("=== ECMAScript 2023 Features ===");
               
               // 1. Array `findLast` and `findLastIndex`
               const numbers = [1, 2, 3, 4, 5, 6];
               log("findLast even: " + numbers.findLast(n => n % 2 === 0));         // 6
               log("findLastIndex even: " + numbers.findLastIndex(n => n % 2 === 0)); // 5
               
               // 2. `toSorted`, `toReversed`, `toSpliced`
               const original = [5, 3, 1, 4];
               log("original: " + original);
               log("toSorted: " + original.toSorted((a, b) => a - b)); // [1, 3, 4, 5]
               log("toReversed: " + original.toReversed());            // [4, 1, 3, 5]
               log("toSpliced: " + original.toSpliced(1, 2, 9, 8));     // [5, 9, 8, 4]
               
               // 3. `Symbol.prototype.description`
               const sym = Symbol("hello");
               log("Symbol description: " + sym.description); // "hello"
               
               // 4. `ArrayBuffer.prototype.transfer` (stage 3, may be partially implemented)
               try {
                   const buffer = new ArrayBuffer(8);
                   const view = new Uint8Array(buffer);
                   view[0] = 42;
                   // May or may not be supported in Jint depending on version
                   const newBuffer = buffer.transfer(16);
                   log("Transferred buffer byteLength: " + newBuffer.byteLength);
               } catch (e) {
                   log("ArrayBuffer.transfer unsupported: " + e.message);
               }
               
               // 5. Hashbang grammar (not runnable in runtime eval, but part of ES2023)
               /*
               #! /usr/bin/env node
               console.log("Hashbang ignored in compliant environments");
               */
               
               // 6. `Change Array.prototype` methods to not return `this`
               const result2 = [1, 2, 3].with(1, 99); // proposed in ES2022, included in ES2023 final
               log("Array.with: " + result2); // [1, 99, 3]
               
               // 7. Error cause
               try {
                   try {
                       throw new Error("Original error");
                   } catch (e) {
                       throw new Error("Wrapped error", { cause: e });
                   }
               } catch (e) {
                   log("Error: " + e.message);
                   log("Cause: " + (e.cause ? e.cause.message : "none"));
               }
               """);
        var contributors = new List<Contributor>
        {
            new Contributor("alice", 50, 10000, 2000, DateTime.UtcNow.AddMonths(-15), DateTime.UtcNow.AddMonths(-2)),
            new Contributor("bob", 120, 5000, 4000, DateTime.UtcNow.AddMonths(-30), DateTime.UtcNow.AddMonths(-8)),
            new Contributor("carol", 10, 2000, 100, DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddDays(-10)),
            new Contributor("dave", 5, 1000, 500, DateTime.UtcNow.AddMonths(-50), DateTime.UtcNow.AddMonths(-40)),
            new Contributor("eve", 80, 3000, 1000, DateTime.UtcNow.AddMonths(-4), DateTime.UtcNow.AddMonths(-1)),
            new Contributor("frank", 60, 1500, 800, DateTime.UtcNow.AddMonths(-10), DateTime.UtcNow.AddMonths(-10)),
            new Contributor("grace", 35, 4000, 2500, DateTime.UtcNow.AddMonths(-20), DateTime.UtcNow.AddMonths(-2)),
            new Contributor("heidi", 100, 7000, 3000, DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow.AddMonths(-3)),
            new Contributor("ivan", 25, 1000, 900, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddDays(-5)),
            new Contributor("judy", 8, 500, 300, DateTime.UtcNow.AddMonths(-8), DateTime.UtcNow.AddMonths(-7)),
            new Contributor("brycen", 300, 400, 700, DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMinutes(-20)),
            new Contributor("dark brycen", 150, 9000, 9500, DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddMinutes(-10)),
            new Contributor("jaex", 2000, 3300000, 2500000, DateTime.UtcNow.AddYears(-10), DateTime.UtcNow.AddDays(-18)),
    new Contributor("McoreD", 273, 36316, 26185, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("L1Q", 146, 30805, 7865, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("sylveon", 113, 28976, 47498, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("SupSuper", 31, 1497, 1140, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("campbeb", 26, 7486, 3270, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("nielsmh", 25, 1116, 420, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("123jimin", 21, 15730, 222, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Shadorc", 20, 5734, 1135, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("alanedwardes", 19, 1015, 501, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("lithium720", 17, 681, 227, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("kanaxx", 17, 16855, 65, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Fyers", 17, 1906, 92, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("DanielMcAssey", 16, 23801, 13731, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("davidruhmann", 16, 1096, 506, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Xinayder", 16, 10004, 236, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("gpailler", 16, 858, 553, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("athosmoreno", 14, 3334, 3, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("joohyungvip", 13, 2205, 731, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Scrxtchy", 12, 902, 580, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("tinybarks", 12, 21889, 20500, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("uncled1023", 11, 21519, 20526, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("k3d3", 10, 679, 289, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Lego6245", 10, 20956, 20719, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("stuntguy3000", 9, 636, 88, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("dannoe", 9, 247, 90, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Serega124", 8, 4867, 638, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("alantsai", 8, 13441, 1, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("erelado", 8, 18159, 80, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("RikoDEV", 8, 18308, 158, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("kusc-leica", 8, 174, 27, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Ninn0x4F", 8, 13, 12, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("barsv", 8, 184, 56, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("mstojcevich", 7, 470, 95, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("wi5nia", 7, 25616, 25079, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("mxschmitt", 7, 1790, 641, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("spidersouris", 7, 458, 451, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("gregorygregio", 7, 427, 123, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("pjammo", 6, 27246, 13321, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Jefemy", 6, 17, 11, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("RedMser", 6, 174, 125, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("etoensfeldt", 6, 91, 43, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Creekie1337", 6, 517, 2, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("petrosian", 5, 3833, 3571, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("Sneer-Cat", 5, 2328, 412, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("ovnisoftware", 5, 10958, 60, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("maboroshin", 5, 3038, 338, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("pourmand1376", 5, 13844, 39, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("SolidX", 4, 45, 35, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor("simonporte", 4, 4592, 260, new DateTime(2013,10,1), DateTime.MinValue),
    new Contributor(
        UserName: "janitorbot",
        CommitCount: 75,
        LinesAdded: 200,
        LinesRemoved: 8000,
        FirstContributionDate: DateTime.UtcNow.AddYears(-1).AddMonths(-2),
        LastContributionDate: DateTime.UtcNow.AddDays(-5)
    ),
    new Contributor(
        UserName: "masscleanerbot", // A descriptive name for this type of contributor
        CommitCount: 1500, // Example commit count
        LinesRemoved: 1000000,
        LinesAdded: 10000000,
        FirstContributionDate: DateTime.UtcNow.AddYears(-5), // Example date
        LastContributionDate: DateTime.UtcNow.AddDays(-1) // Example date
    ),
    new Contributor(
        UserName: "MegaRefactorer", // A name suggesting large-scale, infrequent changes
        CommitCount: 25, // Very low commit count
        LinesAdded: 500000, // Half a million lines added in few commits (e.g., new module, major rewrite)
        LinesRemoved: 2000000, // Two million lines removed (e.g., deprecating old systems, massive cleanup)
        FirstContributionDate: DateTime.UtcNow.AddYears(-7), // Long-standing, but not daily activity
        LastContributionDate: DateTime.UtcNow.AddYears(-6) // Still occasionally active
    ),

    new Contributor("dependabot[bot]", 400, 800, 750, DateTime.UtcNow.AddYears(-3), DateTime.UtcNow.AddMinutes(-15)),
    new Contributor("renovatebot[bot]", 500, 1200, 1100, DateTime.UtcNow.AddYears(-4), DateTime.UtcNow.AddHours(-1)),
    new Contributor(
        "BrycensRanch", 
        626,                // commits
        200_140,            // lines added
        715_001,            // lines removed
        DateTime.UtcNow.AddYears(-1),  // assuming started about 8 years ago
        DateTime.UtcNow.AddDays(-1)     // last contribution 1 day ago
    ),

        };
        
        Console.WriteLine("Verified Publishers:");
        var verified = VerifiedPublisherSelector.DetermineVerifiedPublishers(contributors);

        foreach (var v in verified)
        {
            Console.WriteLine($"{v.UserName} - Score: {v.Score:F2} ({v.Reason})");
        }
    }
}
