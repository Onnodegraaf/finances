using System.Text;

namespace Pages;

public sealed class Style
{
    public static string Render(IEnumerable<IGrouping<string, Label>> labels)
    {
        var sb = new StringBuilder("""
        #label-selector:has(#label-checkbox-everything:checked) ~ form tfoot tr:not(.label-all) {
                visibility: collapse;
        }
        """);
        foreach(var label in labels)
        {
            sb.AppendFormat("""
#label-selector:has(#label-checkbox-{0}:checked) ~ form table:not(thead) tr:not(.label-{0}) {{
        visibility: collapse;
}}
""", label.Key);
        }

        return $$"""
        <style> 
            .currency {
                    text-align: right;
                }
            input[name="Description"] {field-sizing: content; }

            /* Hide the actual checkbox input */
            input[name="label-selector"] {
                display: none;
            }

            label:has(input[type="radio"]:checked) {
                background-color: #d0d0f5;
                font-size: 1.3em;
                border-radius: 10px;
                padding: 5px 10px;
            }

            /* Show the content when the checkbox is checked, using the general sibling selector (~) */
            {{sb.ToString()}}

            table {
                --color: #d0d0f5;
                text-align: left;
                border-collapse: collapse;  
            }

            input[readonly] {
                background: none;
                border: 0;
            }
            tr {
                border-bottom: 1px solid;
            }
            th,
            td {
                border: 1px solid;
            }
            thead,
            tfoot {
                background: var(--color);
            }

            tbody tr:nth-child(even) {
                background: color-mix(in srgb, var(--color), transparent 60%);
            }

            span.in::after {
                content: "▲";
                font-size: large;
                color: green;
            }
            #calendar-grouping span {
                display: block;
            }
            span.out::after {
                content: "▼";
                font-size: large;
                color: red;
            }

            #calendar-grouping {
                display: grid;
                grid-template-columns: repeat(7, auto);
            }

            #calendar-grouping div {
                border: 10px black;
                border-radius: 10px;
                background-color: aliceblue;
                margin: 1px;
                padding: 5px;
            }
            tr.label-row td, tr.label-row th {
                padding-top: 15px;
             }
            table.summary td, table.summary th {
                padding-left: 15px;
            }
            table.summary td {
                text-align: right;
                border: none;
            }
            table.summary th {
                border: none;
            }
            .sub-label-row th {
                text-align: right;
            }
            tr.sub-label-row {
                font-size: smaller;
            }
            .label-row {
                text-align: left;
            }
            table.summary tr:not(:has(span)) {
                visibility: collapse;
            }



            ul {
            list-style-type: none;
            margin: 0;
            padding: 0;
            overflow: hidden;
            background-color: #333333;
            }

            ul li {
            float: left;
            }

            ul li a {
            display: block;
            color: white;
            text-align: center;
            padding: 14px 16px;
            text-decoration: none;
            }

            ul li a:hover {
            background-color: #111111;
            }

            ul li a.active {
            background-color: #04AA6D;
            }

    </style>
    """;
    }
}