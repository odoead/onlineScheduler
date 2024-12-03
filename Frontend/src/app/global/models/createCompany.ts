import { TimeSpan } from "./timespan";

export interface CreateCompany
{
    name: string,
    description: string,
    openingTimeLOC: TimeSpan,
    closingTimeLOC: TimeSpan,
    companyType: number,
    workingDays: number[],
    latitude: number,
    longitude: number,
    ownerEmail: string
}