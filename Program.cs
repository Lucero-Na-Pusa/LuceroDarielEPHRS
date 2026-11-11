using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class Employee
{
    public int Id { get; set; }                       // Column 1
    public string FirstName { get; set; } = "";       // Column 2
    public string LastName { get; set; } = "";        // Column 3
    public string Department { get; set; } = "";      // Column 4
    public string JobTitle { get; set; } = "";        // Column 5
    public decimal MonthlySalary { get; set; }        // Column 6
    public double TaxRate { get; set; }               // Column 7
    public DateTime HireDate { get; set; }            // Column 8
    public bool IsActive { get; set; }                // Column 9
    public string Email { get; set; } = "";           // Column 10

    // Computed (not a data column): take-home pay after tax
    public decimal NetSalary => MonthlySalary - (MonthlySalary * (decimal)TaxRate);
}

class Program
{
    // Starts EMPTY (0 pre-defined records)
    static List<Employee> employees = new List<Employee>();

    // Auto-increment starting at 1001
    static int nextId = 1001;

    static void Main(string[] args)
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.WriteLine("=== EPHRS - MAIN BRANCH VERSION ===");
            Console.WriteLine("1. Create (Add Employee)");
            Console.WriteLine("2. Read (View All Employees)");
            Console.WriteLine("3. Search / Filter");
            Console.WriteLine("4. Update Employee");
            Console.WriteLine("5. Delete Employee");
            Console.WriteLine("6. Exit");
            Console.Write("\nSelect an option: ");

            string choice = (Console.ReadLine() ?? "").Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    CreateEmployee();
                    break;
                case "2":
                    ReadEmployees();
                    break;
                case "3":
                    SearchEmployees();
                    break;
                case "4":
                    UpdateEmployee();
                    break;
                case "5":
                    DeleteEmployee();
                    break;
                case "6":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            if (running)
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }
    }

    // ---------------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------------
    static void CreateEmployee()
    {
        Console.WriteLine("--- Add New Employee ---");

        var emp = new Employee
        {
            Id = nextId++,
            FirstName = PromptRequired("First Name"),
            LastName = PromptRequired("Last Name"),
            Department = PromptRequired("Department (e.g., IT, HR, Sales, Finance)"),
            JobTitle = PromptRequired("Job Title"),
            MonthlySalary = PromptSalary("Monthly Salary"),
            TaxRate = PromptTaxRate("Tax Rate (e.g., 0.12 for 12%)"),
            HireDate = PromptDate("Hire Date (yyyy-MM-dd)"),
            IsActive = PromptActive("Is Active? (Y/N)"),
            Email = PromptEmail("Company Email")
        };

        employees.Add(emp);

        Console.WriteLine($"\nEmployee added successfully! Assigned ID: {emp.Id}");
    }

    // ---------------------------------------------------------------
    // READ
    // ---------------------------------------------------------------
    static void ReadEmployees()
    {
        Console.WriteLine("--- Employee Records ---");

        if (employees.Count == 0)
        {
            Console.WriteLine("No employees found.");
            return;
        }

        PrintEmployees(employees.OrderBy(e => e.Id).ToList());

        // Payroll summary using LINQ aggregates
        Console.WriteLine("=== PAYROLL SUMMARY ===");
        Console.WriteLine($"Total employees : {employees.Count}");
        Console.WriteLine($"Active          : {employees.Count(e => e.IsActive)}");
        Console.WriteLine($"Total gross     : {employees.Sum(e => e.MonthlySalary):N2}");
        Console.WriteLine($"Total net       : {employees.Sum(e => e.NetSalary):N2}");
    }

    // ---------------------------------------------------------------
    // SEARCH / FILTER
    // ---------------------------------------------------------------
    static void SearchEmployees()
    {
        Console.WriteLine("--- Search / Filter ---");
        Console.WriteLine("1. By name (first or last)");
        Console.WriteLine("2. By department");
        Console.WriteLine("3. By status (Active / Inactive)");
        Console.WriteLine("4. By minimum monthly salary");
        Console.Write("\nChoose a filter: ");

        string option = (Console.ReadLine() ?? "").Trim();
        List<Employee> results;

        switch (option)
        {
            case "1":
                string keyword = PromptRequired("Enter name keyword");
                results = employees
                    .Where(e => e.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                             || e.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                break;

            case "2":
                string dept = PromptRequired("Enter department");
                results = employees
                    .Where(e => e.Department.Equals(dept, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                break;

            case "3":
                bool active = PromptActive("Show Active employees? (Y = Active, N = Inactive)");
                results = employees.Where(e => e.IsActive == active).ToList();
                break;

            case "4":
                decimal min = PromptSalary("Minimum monthly salary");
                results = employees.Where(e => e.MonthlySalary >= min).ToList();
                break;

            default:
                Console.WriteLine("\nInvalid filter.");
                return;
        }

        Console.WriteLine();
        if (results.Count == 0)
        {
            Console.WriteLine("No matching employees found.");
            return;
        }

        PrintEmployees(results.OrderBy(e => e.Id).ToList());
        Console.WriteLine($"{results.Count} record(s) found.");
    }

    // ---------------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------------
    static void UpdateEmployee()
    {
        if (employees.Count == 0)
        {
            Console.WriteLine("No employees to update.");
            return;
        }

        ReadEmployees();

        Console.Write("\nEnter the ID of the employee to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("\nInvalid ID format.");
            return;
        }

        Employee emp = employees.FirstOrDefault(e => e.Id == id);
        if (emp == null)
        {
            Console.WriteLine("\nEmployee not found.");
            return;
        }

        Console.WriteLine("\n(Press Enter on any field to keep its current value)\n");

        string input;

        input = Ask($"First Name [{emp.FirstName}]");
        if (!string.IsNullOrWhiteSpace(input)) emp.FirstName = input;

        input = Ask($"Last Name [{emp.LastName}]");
        if (!string.IsNullOrWhiteSpace(input)) emp.LastName = input;

        input = Ask($"Department [{emp.Department}]");
        if (!string.IsNullOrWhiteSpace(input)) emp.Department = input;

        input = Ask($"Job Title [{emp.JobTitle}]");
        if (!string.IsNullOrWhiteSpace(input)) emp.JobTitle = input;

        input = Ask($"Monthly Salary [{emp.MonthlySalary:N2}]");
        if (decimal.TryParse(input, out decimal newSalary) && newSalary > 0)
            emp.MonthlySalary = newSalary;

        input = Ask($"Tax Rate [{emp.TaxRate}]");
        if (double.TryParse(input, out double newTax) && newTax >= 0 && newTax <= 1)
            emp.TaxRate = newTax;

        input = Ask($"Hire Date (yyyy-MM-dd) [{emp.HireDate:yyyy-MM-dd}]");
        if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out DateTime newDate))
            emp.HireDate = newDate;

        input = Ask($"Is Active? Y/N [{(emp.IsActive ? "Y" : "N")}]");
        if (input.Equals("Y", StringComparison.OrdinalIgnoreCase)) emp.IsActive = true;
        else if (input.Equals("N", StringComparison.OrdinalIgnoreCase)) emp.IsActive = false;

        input = Ask($"Email [{emp.Email}]");
        if (!string.IsNullOrWhiteSpace(input) && input.Contains('@')) emp.Email = input;

        Console.WriteLine("\nEmployee updated successfully!");
    }

    // ---------------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------------
    static void DeleteEmployee()
    {
        if (employees.Count == 0)
        {
            Console.WriteLine("No employees to delete.");
            return;
        }

        ReadEmployees();

        Console.Write("\nEnter the ID of the employee to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("\nInvalid ID format.");
            return;
        }

        Employee emp = employees.FirstOrDefault(e => e.Id == id);
        if (emp == null)
        {
            Console.WriteLine("\nEmployee not found.");
            return;
        }

        Console.Write($"Delete {emp.FirstName} {emp.LastName}? (Y/N): ");
        string confirm = (Console.ReadLine() ?? "").Trim();

        if (confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            employees.RemoveAll(e => e.Id == id);
            Console.WriteLine("\nEmployee deleted successfully!");
        }
        else
        {
            Console.WriteLine("\nDeletion cancelled.");
        }
    }

    // ---------------------------------------------------------------
    // DISPLAY HELPER (all 10 columns per employee)
    // ---------------------------------------------------------------
    static void PrintEmployees(List<Employee> list)
    {
        foreach (var e in list)
        {
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"ID          : {e.Id}");
            Console.WriteLine($"First Name  : {e.FirstName}");
            Console.WriteLine($"Last Name   : {e.LastName}");
            Console.WriteLine($"Department  : {e.Department}");
            Console.WriteLine($"Job Title   : {e.JobTitle}");
            Console.WriteLine($"Salary      : {e.MonthlySalary:N2}");
            Console.WriteLine($"Tax Rate    : {e.TaxRate:P1}");
            Console.WriteLine($"Hire Date   : {e.HireDate:yyyy-MM-dd}");
            Console.WriteLine($"Status      : {(e.IsActive ? "Active" : "Inactive")}");
            Console.WriteLine($"Email       : {e.Email}");
            Console.WriteLine($"Net Salary  : {e.NetSalary:N2}");
        }
        Console.WriteLine(new string('-', 50));
    }

    // ---------------------------------------------------------------
    // INPUT HELPERS (keep asking until the input is valid)
    // ---------------------------------------------------------------
    static string Ask(string label)
    {
        Console.Write($"{label}: ");
        return (Console.ReadLine() ?? "").Trim();
    }

    static string PromptRequired(string label)
    {
        while (true)
        {
            string input = Ask(label);
            if (!string.IsNullOrWhiteSpace(input)) return input;
            Console.WriteLine("  This field is required.");
        }
    }

    static decimal PromptSalary(string label)
    {
        while (true)
        {
            if (decimal.TryParse(Ask(label), out decimal value) && value > 0) return value;
            Console.WriteLine("  Enter a valid amount greater than 0.");
        }
    }

    static double PromptTaxRate(string label)
    {
        while (true)
        {
            if (double.TryParse(Ask(label), out double value) && value >= 0 && value <= 1) return value;
            Console.WriteLine("  Enter a decimal between 0 and 1 (e.g., 0.12).");
        }
    }

    static DateTime PromptDate(string label)
    {
        while (true)
        {
            if (DateTime.TryParseExact(Ask(label), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out DateTime value)) return value;
            Console.WriteLine("  Use the format yyyy-MM-dd (e.g., 2024-06-15).");
        }
    }

    static bool PromptActive(string label)
    {
        while (true)
        {
            string input = Ask(label);
            if (input.Equals("Y", StringComparison.OrdinalIgnoreCase)) return true;
            if (input.Equals("N", StringComparison.OrdinalIgnoreCase)) return false;
            Console.WriteLine("  Enter Y or N.");
        }
    }

    static string PromptEmail(string label)
    {
        while (true)
        {
            string input = Ask(label);
            if (input.Contains('@') && input.Contains('.')) return input;
            Console.WriteLine("  Enter a valid email (e.g., name@company.com).");
        }
    }
}
