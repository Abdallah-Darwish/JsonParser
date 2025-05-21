namespace JsonParser.Tests;

public class TestParser
{
    [Fact]
    public void Parse_ComplicatedJson_ParsesFine()
    {
        string src = """
{"1": 2,
            "xyz": [{"abc": [1, 2, 3], 
            
            
            
            "\u0042": 
            
            
            [12, "abc",
            
            
            
             null, false,
             
             
             
              true]}, {     },
              
              
              
              
              
              
              
1.2e-1]}
""";
        var actual = Utility.Print(src);
        string expected = """
{
	"1": 2,
	"xyz": [
		{
			"abc": [
				1,
				2,
				3
			],
			"B": [
				12,
				"abc",
				null,
				false,
				true
			]
		},
		{},
		0.12
	]
}
""";

        Assert.Equal(expected, actual);
    }
}
