using FluentAssertions;
using ALOud.Services.Infrastructure.ExpertSystem;
using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Tests.Services.Infrastructure.ExpertSystem;

public class ExpertSystemServiceTests
{
    private readonly ExpertSystemService _service;

    public ExpertSystemServiceTests()
    {
        _service = new ExpertSystemService();
    }

    [Fact]
    public void Evaluate_WithValidProfile_ShouldReturnRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Recommendation>();
    }

    [Fact]
    public void Evaluate_WithHotClimateProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Climate = EClimate.Hot;
        profile.Occasion = EOccasion.Daily;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithColdClimateProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Climate = EClimate.Cold;
        profile.Occasion = EOccasion.Daily;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithHumidClimateProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Climate = EClimate.Humid;
        profile.Occasion = EOccasion.Sport;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithMixedClimateProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Climate = EClimate.Mixed;
        profile.SeasonPreference = ESeasonPreference.AllYear;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithFormalOccasionProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Occasion = EOccasion.Formal;
        profile.Persona = EPersona.Elegant;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithDateOccasionProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Occasion = EOccasion.Date;
        profile.Persona = EPersona.Sexy;
        profile.Compliment = EComplimentDesire.Yes;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithNightlifeOccasionProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Occasion = EOccasion.Nightlife;
        profile.Persona = EPersona.Rebellious;
        profile.WantsLongPerformance = true;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithSportOccasionProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Occasion = EOccasion.Sport;
        profile.Persona = EPersona.Sporty;
        profile.Climate = EClimate.Hot;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithOfficeOccasionProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Occasion = EOccasion.Office;
        profile.Persona = EPersona.Corporate;
        profile.Sensitivity = ESensitivity.PrefersMinimal;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithGymOccasionProfile_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Occasion = EOccasion.Gym;
        profile.Climate = EClimate.Hot;
        profile.SkinType = ESkinType.Oily;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithDrySkinType_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SkinType = ESkinType.Dry;
        profile.Climate = EClimate.Cold;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithOilySkinType_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SkinType = ESkinType.Oily;
        profile.Climate = EClimate.Humid;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithNormalSkinType_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SkinType = ESkinType.Normal;
        profile.SeasonPreference = ESeasonPreference.Spring;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithComplimentDesireYes_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Compliment = EComplimentDesire.Yes;
        profile.Persona = EPersona.Sexy;
        profile.Occasion = EOccasion.Date;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithComplimentDesireNo_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Compliment = EComplimentDesire.No;
        profile.Persona = EPersona.Minimalist;
        profile.Occasion = EOccasion.Office;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithComplimentDesireNeutral_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Compliment = EComplimentDesire.Neutral;
        profile.Persona = EPersona.Artistic;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithSpringSeasonPreference_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SeasonPreference = ESeasonPreference.Spring;
        profile.Climate = EClimate.Mixed;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithSummerSeasonPreference_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SeasonPreference = ESeasonPreference.Summer;
        profile.Climate = EClimate.Hot;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithFallSeasonPreference_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SeasonPreference = ESeasonPreference.Fall;
        profile.Climate = EClimate.Cold;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithWinterSeasonPreference_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SeasonPreference = ESeasonPreference.Winter;
        profile.Climate = EClimate.Cold;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithAllYearSeasonPreference_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SeasonPreference = ESeasonPreference.AllYear;
        profile.Climate = EClimate.Mixed;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithCorporatePersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Corporate;
        profile.Occasion = EOccasion.Office;
        profile.Compliment = EComplimentDesire.Neutral;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithSexyPersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Sexy;
        profile.Occasion = EOccasion.Date;
        profile.Compliment = EComplimentDesire.Yes;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithSportyPersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Sporty;
        profile.Occasion = EOccasion.Sport;
        profile.Climate = EClimate.Hot;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithArtisticPersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Artistic;
        profile.Occasion = EOccasion.Daily;
        profile.Compliment = EComplimentDesire.Neutral;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithMinimalistPersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Minimalist;
        profile.Sensitivity = ESensitivity.PrefersMinimal;
        profile.Compliment = EComplimentDesire.No;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithRebelliousPersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Rebellious;
        profile.Occasion = EOccasion.Nightlife;
        profile.Compliment = EComplimentDesire.Yes;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithElegantPersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Elegant;
        profile.Occasion = EOccasion.Formal;
        profile.SeasonPreference = ESeasonPreference.Fall;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithYouthfulPersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Youthful;
        profile.SeasonPreference = ESeasonPreference.Spring;
        profile.Occasion = EOccasion.Daily;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithMaturePersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Mature;
        profile.SeasonPreference = ESeasonPreference.Winter;
        profile.Occasion = EOccasion.Formal;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithMysteriousPersona_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Persona = EPersona.Mysterious;
        profile.Occasion = EOccasion.Date;
        profile.Climate = EClimate.Cold;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithNoSensitivity_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Sensitivity = ESensitivity.None;
        profile.Compliment = EComplimentDesire.Yes;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithMigraineSensitivity_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Sensitivity = ESensitivity.Migraine;
        profile.Persona = EPersona.Minimalist;
        profile.Compliment = EComplimentDesire.No;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithHatesSweetSensitivity_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Sensitivity = ESensitivity.HatesSweet;
        profile.Persona = EPersona.Corporate;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithHatesSpiceSensitivity_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Sensitivity = ESensitivity.HatesSpice;
        profile.Climate = EClimate.Cold;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithHatesFloralSensitivity_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Sensitivity = ESensitivity.HatesFloral;
        profile.Persona = EPersona.Rebellious;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithHatesFreshSensitivity_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Sensitivity = ESensitivity.HatesFresh;
        profile.SeasonPreference = ESeasonPreference.Winter;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithPrefersMinimalSensitivity_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Sensitivity = ESensitivity.PrefersMinimal;
        profile.Persona = EPersona.Minimalist;
        profile.Occasion = EOccasion.Office;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithWantsLongPerformanceTrue_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.WantsLongPerformance = true;
        profile.Occasion = EOccasion.Formal;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithWantsLongPerformanceFalse_ShouldReturnAppropriateRecommendation()
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.WantsLongPerformance = false;
        profile.Occasion = EOccasion.Daily;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Theory]
    [InlineData(EClimate.Hot, EOccasion.Sport)]
    [InlineData(EClimate.Cold, EOccasion.Formal)]
    [InlineData(EClimate.Humid, EOccasion.Daily)]
    [InlineData(EClimate.Mixed, EOccasion.Office)]
    public void Evaluate_WithVariousClimateOccasionCombinations_ShouldReturnValidRecommendations(
        EClimate climate, EOccasion occasion)
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Climate = climate;
        profile.Occasion = occasion;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Theory]
    [InlineData(ESkinType.Dry, EPersona.Elegant)]
    [InlineData(ESkinType.Oily, EPersona.Sporty)]
    [InlineData(ESkinType.Normal, EPersona.Corporate)]
    public void Evaluate_WithVariousSkinTypePersonaCombinations_ShouldReturnValidRecommendations(
        ESkinType skinType, EPersona persona)
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.SkinType = skinType;
        profile.Persona = persona;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Theory]
    [InlineData(ESensitivity.Migraine, EComplimentDesire.No)]
    [InlineData(ESensitivity.HatesSweet, EComplimentDesire.Neutral)]
    [InlineData(ESensitivity.HatesSpice, EComplimentDesire.Yes)]
    [InlineData(ESensitivity.HatesFloral, EComplimentDesire.No)]
    [InlineData(ESensitivity.HatesFresh, EComplimentDesire.Neutral)]
    [InlineData(ESensitivity.PrefersMinimal, EComplimentDesire.No)]
    public void Evaluate_WithVariousSensitivityComplimentCombinations_ShouldReturnValidRecommendations(
        ESensitivity sensitivity, EComplimentDesire compliment)
    {
        // Arrange
        var profile = CreateValidUserProfile();
        profile.Sensitivity = sensitivity;
        profile.Compliment = compliment;

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_WithComplexProfile_ShouldReturnComprehensiveRecommendation()
    {
        // Arrange
        var profile = new UserProfileDto
        {
            Climate = EClimate.Hot,
            Occasion = EOccasion.Date,
            SkinType = ESkinType.Oily,
            Compliment = EComplimentDesire.Yes,
            SeasonPreference = ESeasonPreference.Summer,
            Persona = EPersona.Sexy,
            Sensitivity = ESensitivity.HatesSweet,
            WantsLongPerformance = true
        };

        // Act
        var result = _service.Evaluate(profile);

        // Assert
        result.Should().NotBeNull();
        result.Prefer.Should().NotBeNull();
        result.Avoid.Should().NotBeNull();
        result.Reasons.Should().NotBeNull();
        // For complex profiles, we expect the expert system to provide detailed recommendations
    }

    [Fact]
    public void Evaluate_ShouldReturnNewRecommendationInstanceEachTime()
    {
        // Arrange
        var profile = CreateValidUserProfile();

        // Act
        var result1 = _service.Evaluate(profile);
        var result2 = _service.Evaluate(profile);

        // Assert
        result1.Should().NotBeSameAs(result2);
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
    }

    [Fact]
    public async Task Evaluate_ShouldHandleConcurrentRequests()
    {
        // Arrange
        var profile1 = CreateValidUserProfile();
        profile1.Climate = EClimate.Hot;
        
        var profile2 = CreateValidUserProfile();
        profile2.Climate = EClimate.Cold;
        
        var tasks = new List<Task<Recommendation>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var profile = i % 2 == 0 ? profile1 : profile2;
            tasks.Add(Task.Run(() => _service.Evaluate(profile)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyContain(r => r != null);
        results.Should().OnlyContain(r => r.Prefer != null);
        results.Should().OnlyContain(r => r.Avoid != null);
        results.Should().OnlyContain(r => r.Reasons != null);
    }

    private static UserProfileDto CreateValidUserProfile()
    {
        return new UserProfileDto
        {
            Climate = EClimate.Mixed,
            Occasion = EOccasion.Daily,
            SkinType = ESkinType.Normal,
            Compliment = EComplimentDesire.Neutral,
            SeasonPreference = ESeasonPreference.AllYear,
            Persona = EPersona.Corporate,
            Sensitivity = ESensitivity.None,
            WantsLongPerformance = false
        };
    }
}