﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TeslaTizen.Data;
using TeslaTizen.Models;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Linq;
using System.Threading.Tasks;

namespace TeslaTizen.Services
{
    public class ProfilesService: IProfileService
    {
        private const string ProfilesKey = "Profiles";

        private readonly ICache Cache;
        private BehaviorSubject<List<Profile>> ProfilesSubject;
        private List<Profile> ProfilesCache;

        public ProfilesService() : this(CacheFactory.CreateCache()) { }

        public ProfilesService(ICache cache)
        {
            Cache = cache;
        }

        public async Task<IObservable<List<Profile>>> GetProfilesAsync()
        {
            await HydrateCache();
            return ProfilesSubject.FirstAsync();
        }

        public async Task UpsertProfileAsync(Profile profile)
        {
            await HydrateCache();
            var existingProfile = ProfilesCache.FirstOrDefault(p => p.Id == profile.Id);
            if (existingProfile != null)
            {
                existingProfile.Name = profile.Name;
                existingProfile.Actions = profile.Actions;
            }
            else
            {
                ProfilesCache.Add(profile);
            }
            await Cache.StoreDataAsync(ProfilesCache, ProfilesKey);
            UpdateSubject();
        }

        public async Task DeleteProfileAsync(Profile profile)
        {
            await HydrateCache();
            var existingProfile = ProfilesCache.FirstOrDefault(p => p.Id == profile.Id);
            ProfilesCache.Remove(existingProfile);
            await Cache.StoreDataAsync(new List<Profile>(ProfilesCache.ToList()), ProfilesKey);
            UpdateSubject();
        }
        
        private async Task HydrateCache()
        {
            if (ProfilesCache == null)
            {
                ProfilesCache = await Cache.GetDataAsync<List<Profile>>(ProfilesKey) ?? new List<Profile>();
                ProfilesSubject = new BehaviorSubject<List<Profile>>(ProfilesCache);
            }
        }

        private void UpdateSubject()
        {
            ProfilesSubject.OnNext(ProfilesCache);
        }
    }
}
