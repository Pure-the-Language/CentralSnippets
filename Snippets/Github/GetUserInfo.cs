/* Get User Info
Version: 0.1.0

Provides a one-shot query to get user repository and organization information. This is a more straightforward interface compared to Github website, which is manual and only shows limited number of things.

For organization, per https://github.com/orgs/community/discussions/45996, Make sure that the token has the `read:org` scope.
*/

Import(Octokit)

#region Types
public record RepositoryIdentity(string Organization, string Repository);
#endregion

#region Configurations
private const string ClientHeader = "DemoApp";
#endregion

#region Methods
public static void PrintSummary(string userToken)
{
    GitHubClient client = new(new ProductHeaderValue(ClientHeader))
    {
        Credentials = new Credentials(userToken)
    };

    User user = client.User.Current().Result;
    PrintUser(user);

    IReadOnlyList<Repository> repos = client.Repository.GetAllForCurrent().Result;
    {
        Console.WriteLine("Repositories: ");
        PrintRepos(repos);
    }

    IReadOnlyList<Organization> orgs = client.Organization.GetAllForCurrent().Result;
    if (orgs.Count == 0 )
        Console.WriteLine("Make sure your token hsa `read:org` scope.");
    else
    {
        Console.WriteLine("Organizations: ");
        PrintOrgs(orgs);
    }
}
public static RepositoryIdentity[] GetRepositories(string userToken)
{
    GitHubClient client = new(new ProductHeaderValue(ClientHeader))
    {
        Credentials = new Credentials(userToken)
    };

    User user = client.User.Current().Result;
    IReadOnlyList<Repository> repos = client.Repository.GetAllForCurrent().Result;
    return repos.Select(r => new RepositoryIdentity(r.Owner.Login, r.Name)).ToArray();
}

public static string[] GetOrganizations(string userToken)
{
    GitHubClient client = new(new ProductHeaderValue(ClientHeader))
    {
        Credentials = new Credentials(userToken)
    };

    User user = client.User.Current().Result;
    IReadOnlyList<Organization> orgs = client.Organization.GetAllForCurrent().Result;
    return orgs.Select(o => o.Login).ToArray();
}
#endregion

#region Helpers
private static void PrintUser(User user)
{
    Console.WriteLine($"""
        Name: {user.Name}
        Login: {user.Login}
        Public Repos: {user.PublicRepos}
        """);
}
private static void PrintRepos(IReadOnlyList<Repository> repos)
{
    foreach (var repo in repos.OrderBy(r => r.Owner.Name))
        Console.WriteLine($"  {repo.Owner.Login}: {repo.Name}");
}
private static void PrintOrgs(IReadOnlyList<Organization> orgs)
{
    foreach (var org in orgs.OrderBy(o => o.Login))
        Console.WriteLine($"  {org.Login}");
}
#endregion

// Doc
WriteLine("""
Type:
  record RepositoryIdentity(string Organization, string Repository)

Methods:
  void PrintSummary(string userToken)
  RepositoryIdentity[] GetRepositories(string userToken)
  string[] GetOrganizations(string userToken)
""");