namespace Measurement.GraphQL;

public static class QueryTemplates
{
    public const string BatchIdByDisplayId = """
                                             query ($id: UUID!) {
                                               displayById(id: $id) {
                                                 batchId
                                               }
                                             }
                                             """;
    
    public const string ScreenSizeQuery = """
                                           query ($id: UUID!) {
                                             displayById(id: $id) {
                                               displayType { screenSize { height width } }
                                             }
                                           }
                                           """;
}